# Detect the actual Desktop path (handles OneDrive redirection)
$desktopPath = [Environment]::GetFolderPath([Environment+SpecialFolder]::Desktop)

# If that still doesn't resolve, fall back to the user profile or the current directory
if (-not (Test-Path $desktopPath)) {
    $desktopPath = $env:USERPROFILE
}
if (-not (Test-Path $desktopPath)) {
    $desktopPath = (Get-Location).Path
}

Write-Host "Desktop path resolved to: $desktopPath"

$html = @'
<!DOCTYPE html>
<html lang="en">
<head>
<meta charset="UTF-8">
<style>
  body { font-family: 'Segoe UI', Arial, sans-serif; max-width: 960px; margin: 40px auto; padding: 20px; color: #222; line-height: 1.6; }
  h1 { font-size: 1.8em; border-bottom: 2px solid #333; padding-bottom: 8px; }
  h2 { font-size: 1.4em; margin-top: 32px; border-bottom: 1px solid #ccc; padding-bottom: 4px; }
  h3 { font-size: 1.15em; margin-top: 24px; }
  table { border-collapse: collapse; width: 100%; margin: 16px 0; font-size: 0.85em; }
  th, td { border: 1px solid #888; padding: 6px 10px; text-align: left; vertical-align: top; }
  th { background-color: #e8e8e8; font-weight: bold; }
  tr:nth-child(even) { background-color: #f6f6f6; }
  code { background-color: #f0f0f0; padding: 1px 4px; border-radius: 3px; font-family: 'Consolas', monospace; font-size: 0.9em; }
  pre { background-color: #f5f5f5; border: 1px solid #ddd; border-radius: 6px; padding: 12px 16px; overflow-x: auto; font-size: 0.82em; line-height: 1.45; }
  ul { padding-left: 24px; }
  li { margin-bottom: 4px; }
  .positive { color: #1a7d1a; }
  .minus { color: #b33; }
  blockquote { border-left: 4px solid #4a90d9; margin: 16px 0; padding: 8px 16px; background-color: #f0f6fc; }
  hr { border: none; border-top: 1px solid #ccc; margin: 24px 0; }
  .recommendation { background-color: #eef7ee; border: 1px solid #6c6; border-radius: 6px; padding: 12px 16px; margin: 16px 0; }
</style>
</head>
<body>

<h1>Architectural Analysis: Simplifying the Settings Model</h1>

<h2>Current State Summary</h2>
<p>Your project currently has:</p>
<table>
<tr><th>Component</th><th>Role</th></tr>
<tr><td><code>IGeminiSettings</code> / <code>IOpenAiSettings</code> / <code>IDeepSeekSettings</code></td><td>Per-provider settings interfaces (each has <code>ApiKey</code> plus preferences)</td></tr>
<tr><td><code>GeminiSettings</code> / <code>OpenAiSettings</code> / <code>DeepSeekSettings</code></td><td>Concrete classes with all fields including <code>ApiKey</code></td></tr>
<tr><td><code>ISettingsProvider</code></td><td>Per-user settings get/save (all fields including <code>ApiKey</code>)</td></tr>
<tr><td><code>FileSettingsProvider</code></td><td>WinForms/Terminal Server: reads defaults from <code>appsettings.json</code>, merges per-user overrides from <code>%APPDATA%\AIFileProcessor\{userId}\settings.json</code> with DPAPI-encrypted <code>ApiKey</code></td></tr>
<tr><td><code>WebSettingsProvider</code></td><td>Web: API keys from HTTP headers, preferences from request body, save is no-op</td></tr>
<tr><td><code>IUserContext</code> / <code>WindowsUserContext</code> / <code>HttpUserContext</code></td><td>Resolves current user ID</td></tr>
<tr><td><code>GeminiSettingsManager</code> / <code>OpenAiSettingsManager</code> / <code>DeepSeekSettingsManager</code></td><td>Thin wrappers combining <code>ISettingsProvider</code> + <code>IUserContext</code></td></tr>
<tr><td>AI Services</td><td>Receive <code>IOptions&lt;T&gt;</code> defaults at construction; per-call overload accepts a settings object; client cache keyed by fingerprint including <code>ApiKey</code></td></tr>
</table>

<blockquote><strong>Key problem</strong>: API keys and per-user preferences are entangled in the same interfaces, the same storage files, and the same provider methods. The web path asks users to send their API keys to the server.</blockquote>

<hr>

<h2>Approach A: API Key Separation via Dedicated Key Provider</h2>

<h3>Concept</h3>
<p>Introduce a new <code>IApiKeyProvider</code> that reads API keys <strong>only</strong> from machine-level sources (environment variables, <code>appsettings.json</code>, or a vault). <code>ISettingsProvider</code> is stripped of <code>ApiKey</code> entirely. AI services receive keys and preferences separately at call time.</p>

<h3>What changes</h3>
<table>
<tr><th>File/Component</th><th>Change</th></tr>
<tr><td>New: <code>IApiKeyProvider</code> / <code>ApiKeyProvider</code></td><td>Reads keys from env vars / <code>IConfiguration</code></td></tr>
<tr><td><code>IDeepSeekSettings</code>, <code>IOpenAiSettings</code>, <code>IGeminiSettings</code></td><td>Remove <code>ApiKey</code> property</td></tr>
<tr><td><code>DeepSeekSettings</code>, <code>OpenAiSettings</code>, <code>GeminiSettings</code></td><td>Remove <code>ApiKey</code> property</td></tr>
<tr><td><code>ISettingsProvider</code></td><td>No longer carries <code>ApiKey</code>; only preferences</td></tr>
<tr><td><code>FileSettingsProvider</code></td><td>Remove all DPAPI encrypt/decrypt code; user files have no secrets</td></tr>
<tr><td><code>WebSettingsProvider</code></td><td>Remove <code>X-*-Api-Key</code> header reading</td></tr>
<tr><td><code>HttpUserContext</code></td><td>Remove API-key-as-identity hash logic</td></tr>
<tr><td><code>SettingsForm</code></td><td>Remove API key text boxes entirely</td></tr>
<tr><td>AI services</td><td>Inject <code>IApiKeyProvider</code>; merge key with per-user prefs at call time</td></tr>
<tr><td><code>ProcessingOrchestrator</code></td><td>May need to pass merged settings or pass key separately</td></tr>
</table>

<h3>Positives</h3>
<ul>
<li class="positive"><strong>Clean separation of concerns</strong>: secrets are never in user files or transit through web requests</li>
<li class="positive"><strong>No encryption code needed</strong>: per-user files contain only preferences</li>
<li class="positive"><strong>Web is automatically secure</strong>: API keys never touch the HTTP request</li>
<li class="positive"><strong>Minimal conceptual change</strong>: the same three settings classes exist, just without <code>ApiKey</code></li>
<li class="positive"><strong>Terminal Server works naturally</strong>: each user's preferences in <code>%APPDATA%\{user}\</code>, keys from machine env vars</li>
<li class="positive"><strong>Client cache fingerprint simplifies</strong>: with shared keys, fingerprint is <code>endpoint:model</code> per machine</li>
</ul>

<h3>Minuses</h3>
<ul>
<li class="minus"><strong>AI service constructors change</strong>: must inject <code>IApiKeyProvider</code> alongside <code>IOptions&lt;T&gt;</code></li>
<li class="minus"><strong><code>ProcessTextAsync(object settings)</code> overload changes</strong>: signature change ripples through callers</li>
<li class="minus"><strong>Two dependencies instead of one</strong>: AI services depend on both <code>IApiKeyProvider</code> and <code>ISettingsProvider</code></li>
<li class="minus"><strong><code>appsettings.json</code> still has all the default preferences</strong>: keys move to env vars — two config sources to manage</li>
</ul>

<hr>

<h2>Approach B: Simplified ISettingsProvider &mdash; ApiKey Always from IConfiguration, Per-User File Has No Secrets</h2>

<h3>Concept</h3>
<p>Keep the existing <code>ISettingsProvider</code> interface and settings classes <strong>as-is</strong> (they still have <code>ApiKey</code>), but change the <strong>behavior</strong>: <code>ApiKey</code> is always populated from <code>IConfiguration</code> (environment variables / <code>appsettings.json</code>), never from the per-user file. Per-user override files store only non-secret preferences. No encryption is needed.</p>

<h3>What changes</h3>
<table>
<tr><th>File/Component</th><th>Change</th></tr>
<tr><td><code>FileSettingsProvider</code></td><td>Remove DPAPI encrypt/decrypt; skip <code>ApiKey</code> when reading/writing user overrides</td></tr>
<tr><td><code>WebSettingsProvider</code></td><td>Remove <code>X-*-Api-Key</code> header reading; <code>ApiKey</code> comes from server env vars</td></tr>
<tr><td><code>HttpUserContext</code></td><td>Remove API-key-as-identity hash logic</td></tr>
<tr><td><code>SettingsForm</code></td><td>Remove API key text boxes; user can only set preferences</td></tr>
<tr><td><code>ProviderSettingsResponse</code></td><td>Strip <code>ApiKey</code> before sending to client</td></tr>
<tr><td>Settings interfaces &amp; classes</td><td><strong>No change</strong></td></tr>
<tr><td>AI services</td><td><strong>No change</strong></td></tr>
<tr><td><code>ISettingsProvider</code></td><td><strong>No change</strong></td></tr>
</table>

<h3>Positives</h3>
<ul>
<li class="positive"><strong>Least disruptive approach</strong>: no interface changes, no new types, no AI service constructor changes</li>
<li class="positive"><strong>No encryption code</strong>: delete all <code>EncryptString</code>/<code>DecryptString</code>/<code>ProtectedData</code> code</li>
<li class="positive"><strong>Per-user files are plain JSON</strong>: no DPAPI, no base64, human-readable, easy to debug</li>
<li class="positive"><strong>Web is secure by default</strong>: API keys come from server environment, never from the client</li>
<li class="positive"><strong>Terminal Server works</strong>: env vars are machine-level, user prefs are per-user in <code>%APPDATA%</code></li>
<li class="positive"><strong><code>SettingsExtensions.CopyOnto</code> already skips empty <code>ApiKey</code></strong>: no changes needed</li>
<li class="positive"><strong>Settings managers unchanged</strong>: all three managers keep working</li>
<li class="positive"><strong>AI services unchanged</strong>: <code>ProcessTextAsync</code> overloads still receive settings with <code>ApiKey</code></li>
</ul>

<h3>Minuses</h3>
<ul>
<li class="minus"><strong><code>ApiKey</code> remains on the settings interfaces</strong>: semantically misleading &mdash; the property exists but users can't set it</li>
<li class="minus"><strong><code>ProviderSettingsResponse</code> must be careful</strong>: needs to strip <code>ApiKey</code> before sending to client</li>
<li class="minus"><strong><code>appsettings.json</code> is the only source for keys</strong>: if you want different keys per provider without env vars, you still need config</li>
<li class="minus"><strong>No structural simplification</strong>: the three settings interfaces, three managers, and three concrete classes all remain</li>
</ul>

<hr>

<h2>Approach C: Unified Settings Model with Base Class</h2>

<h3>Concept</h3>
<p>Collapse the three separate settings interfaces into a single hierarchy. A base <code>AiProviderSettings</code> class holds all common properties (including <code>ApiKey</code> from machine config). Provider-specific classes add only their unique fields. One <code>ISettingsProvider</code> method set replaces three pairs. The <code>SettingsExtensions.CopyOnto</code> logic moves to the base class.</p>

<h3>What changes</h3>
<table>
<tr><th>File/Component</th><th>Change</th></tr>
<tr><td>New: <code>AiProviderSettings</code> base class</td><td>Common properties: <code>ApiKey</code>, <code>Endpoint</code>, <code>ModelName</code>, <code>TimeoutSeconds</code>, <code>MaxTokens</code>, <code>Temperature</code></td></tr>
<tr><td><code>GeminiSettings</code></td><td>Inherits base; keeps Gemini-specific fields (~14 unique properties)</td></tr>
<tr><td><code>OpenAiSettings</code></td><td>Inherits base; only keeps <code>OrganizationId</code></td></tr>
<tr><td><code>DeepSeekSettings</code></td><td>Inherits base; only keeps <code>EnableReasoning</code></td></tr>
<tr><td>Three settings interfaces</td><td>Removed in favor of <code>IAiProviderSettings</code> or kept as markers</td></tr>
<tr><td><code>ISettingsProvider</code></td><td>Simplified to one pair of get/save methods</td></tr>
<tr><td><code>FileSettingsProvider</code></td><td>One generic method; no DPAPI</td></tr>
<tr><td><code>SettingsExtensions.CopyOnto</code></td><td>Moves to base class; one implementation</td></tr>
<tr><td>Three settings managers</td><td>Collapsed into one or eliminated</td></tr>
<tr><td><code>SettingsForm</code></td><td>Simplified with shared common panel + provider-specific panels</td></tr>
<tr><td>AI services</td><td>Receive derived settings class; otherwise unchanged</td></tr>
</table>

<h3>Positives</h3>
<ul>
<li class="positive"><strong>Maximum code reduction</strong>: eliminates ~3 interfaces, ~3 manager classes, 3 <code>CopyOnto</code> implementations, 3 pairs of get/save methods</li>
<li class="positive"><strong>Easy to add new providers</strong>: inherit from base, add unique fields, register</li>
<li class="positive"><strong>DRY at every level</strong>: one settings provider method, one copy/merge method, one save method</li>
<li class="positive"><strong><code>ApiKey</code> clearly belongs to the base</strong>: always from machine config</li>
<li class="positive"><strong>User files are simple and small</strong>: no secrets, no encryption, plain JSON</li>
<li class="positive"><strong>Settings form can share UI</strong>: common fields in one panel; provider-specific extras in a sub-panel</li>
</ul>

<h3>Minuses</h3>
<ul>
<li class="minus"><strong>Most disruptive refactor</strong>: touches every file referencing the three interfaces &mdash; large blast radius</li>
<li class="minus"><strong><code>IGeminiSettings</code> has many unique fields</strong>: Gemini has ~14 unique properties vs. OpenAI's 1 and DeepSeek's 1 &mdash; uneven split</li>
<li class="minus"><strong>Breaking change for <code>ProcessingOrchestrator</code> and callers</strong>: references to specific interfaces must switch to derived types</li>
<li class="minus"><strong>Configuration binding changes</strong>: <code>appsettings.json</code> structure changes; migration needed</li>
<li class="minus"><strong>Test rewrite</strong>: <code>SettingsExtensionsTests</code> and service tests need rewriting</li>
</ul>

<hr>

<h2>Approach D: IConfiguration-Only (No Custom ISettingsProvider)</h2>

<h3>Concept</h3>
<p>Eliminate <code>ISettingsProvider</code>, <code>FileSettingsProvider</code>, <code>WebSettingsProvider</code>, the three settings managers, and <code>IUserContext</code> entirely. Use .NET's built-in <code>IConfiguration</code> layered model for everything. Per-user preferences are loaded as an additional <code>IConfiguration</code> source. API keys come from environment variables. AI services read from <code>IOptions&lt;T&gt;</code> which automatically overlays all sources.</p>

<h3>What changes</h3>
<table>
<tr><th>File/Component</th><th>Change</th></tr>
<tr><td><code>ISettingsProvider</code></td><td><strong>Removed</strong></td></tr>
<tr><td><code>FileSettingsProvider</code></td><td><strong>Removed</strong> &mdash; replaced by per-user JSON file as <code>IConfiguration</code> source</td></tr>
<tr><td><code>WebSettingsProvider</code></td><td><strong>Removed</strong> &mdash; replaced by middleware for per-request prefs</td></tr>
<tr><td><code>IUserContext</code> and implementations</td><td><strong>Removed</strong> for WinForms; simplified for web</td></tr>
<tr><td>Three settings managers</td><td><strong>Removed</strong> &mdash; callers use <code>IOptions&lt;T&gt;</code> directly</td></tr>
<tr><td><code>SettingsExtensions.CopyOnto</code></td><td><strong>Removed</strong> &mdash; <code>IConfiguration</code> overlay handles merging</td></tr>
<tr><td><code>Program.cs</code> (WinForms)</td><td>Add per-user JSON file as config source</td></tr>
<tr><td><code>Program.cs</code> (Web)</td><td>Middleware merges request prefs into <code>IOptions&lt;T&gt;</code> per request</td></tr>
<tr><td><code>SettingsForm</code></td><td>Writes directly to per-user file; triggers config reload</td></tr>
<tr><td>AI services</td><td><strong>Unchanged</strong> &mdash; already use <code>IOptions&lt;T&gt;</code></td></tr>
</table>

<h3>Positives</h3>
<ul>
<li class="positive"><strong>Eliminates the most code</strong>: <code>ISettingsProvider</code>, both implementations, <code>IUserContext</code>, three managers, <code>CopyOnto</code> &mdash; all gone</li>
<li class="positive"><strong>Uses .NET's built-in config system as designed</strong>: <code>IConfiguration</code> layering is the native way to overlay settings</li>
<li class="positive"><strong><code>IOptions&lt;T&gt;</code> already works</strong>: AI services already inject it &mdash; they get merged results automatically</li>
<li class="positive"><strong>No custom merge logic</strong>: <code>IConfiguration</code> binding handles the overlay</li>
<li class="positive"><strong>Per-user file is plain JSON</strong>: no secrets, no encryption</li>
<li class="positive"><strong>Reload on change</strong>: <code>reloadOnChange: true</code> triggers automatic <code>IOptions&lt;T&gt;</code> refresh</li>
<li class="positive"><strong>API keys from env vars</strong>: <code>AddEnvironmentVariables()</code> is already called</li>
</ul>

<h3>Minuses</h3>
<ul>
<li class="minus"><strong>Per-user file path resolved at startup</strong>: changing users mid-process would require a restart (fine on Terminal Server &mdash; each session is separate process)</li>
<li class="minus"><strong>Web is the hardest part</strong>: <code>IOptions&lt;T&gt;</code> is singleton by default; per-request prefs need <code>IOptionsMonitor&lt;T&gt;</code> + custom token source or <code>IOptionsFactory&lt;T&gt;</code> &mdash; non-trivial</li>
<li class="minus"><strong><code>SettingsForm</code> save requires file reload</strong>: race condition between file write and config refresh</li>
<li class="minus"><strong>Loses explicit interface</strong>: <code>ISettingsProvider</code> made the contract explicit and testable; <code>IConfiguration</code> is harder to unit test</li>
<li class="minus"><strong>Per-call settings overload less useful</strong>: may want to remove it &mdash; breaking change for callers</li>
</ul>

<hr>

<h2>Comparison Matrix</h2>
<table>
<tr><th>Criterion</th><th>A: Key Provider</th><th>B: Simplified Provider</th><th>C: Unified Model</th><th>D: IConfiguration-Only</th></tr>
<tr><td><strong>Code reduction</strong></td><td>Moderate</td><td>Low (remove encryption only)</td><td>High (collapse 3&rarr;1)</td><td>Highest (remove most custom code)</td></tr>
<tr><td><strong>Interface changes</strong></td><td>Add <code>IApiKeyProvider</code>; remove <code>ApiKey</code></td><td>None</td><td>Major (new base, remove 3 interfaces)</td><td>Remove <code>ISettingsProvider</code> entirely</td></tr>
<tr><td><strong>AI service changes</strong></td><td>Constructor + call signature</td><td>None</td><td>Call signature (base type)</td><td>None (already uses <code>IOptions&lt;T&gt;</code>)</td></tr>
<tr><td><strong>WinForms changes</strong></td><td>Remove API key text boxes</td><td>Remove API key text boxes</td><td>Refactor settings form panels</td><td>Refactor to write file + reload</td></tr>
<tr><td><strong>Web changes</strong></td><td>Remove header key reading</td><td>Remove header key reading</td><td>Remove header key reading</td><td>Build middleware for per-request prefs</td></tr>
<tr><td><strong>Encryption removal</strong></td><td>Full</td><td>Full</td><td>Full</td><td>Full</td></tr>
<tr><td><strong>Testability</strong></td><td>High (explicit interfaces)</td><td>Highest (minimal change to tests)</td><td>Medium (rewrite tests)</td><td>Low (IConfiguration hard to mock)</td></tr>
<tr><td><strong>Web per-request prefs</strong></td><td>Needs solution</td><td>Already solved (request body)</td><td>Needs solution</td><td>Needs custom IOptionsFactory</td></tr>
<tr><td><strong>Adding a new provider</strong></td><td>Add interface + class + key method</td><td>Add interface + class (same as today)</td><td>Inherit base, add fields</td><td>Add class + config section</td></tr>
<tr><td><strong>Risk of re-introducing user API keys</strong></td><td>Low</td><td>Medium</td><td>Low</td><td>Medium</td></tr>
<tr><td><strong>Blast radius</strong></td><td>Medium</td><td>Low</td><td>High</td><td>High</td></tr>
</table>

<hr>

<h2>Recommendation</h2>
<div class="recommendation">
<p><strong>For your stated goals of simplification with minimal risk, Approach B is the best starting point.</strong> It:</p>
<ol>
<li>Removes all encryption/DPAPI code immediately</li>
<li>Makes per-user files plain JSON (no secrets)</li>
<li>Secures the web path (keys from server env, never from client)</li>
<li>Requires zero interface changes &mdash; the settings classes, <code>ISettingsProvider</code>, AI services, and DI registrations all stay the same</li>
<li>The only real work is deleting code in <code>FileSettingsProvider</code> (remove encrypt/decrypt), <code>WebSettingsProvider</code> (remove header key reading), <code>SettingsForm</code> (remove API key text boxes), and <code>HttpUserContext</code> (remove API-key hash)</li>
</ol>
<p><strong>If you want deeper structural simplification afterward</strong>, Approach C (unified base class) is the natural next step &mdash; it reduces the three-interface/three-manager pattern to one, but it's a larger refactor best done as a second, separate pass.</p>
</div>

</body>
</html>
'@

# Write HTML to a temp file
$htmlPath = Join-Path $env:TEMP "AIFileProcessor_Architectural_Analysis.html"
$html | Out-File -FilePath $htmlPath -Encoding UTF8

# Determine the PDF output path
$pdfPath = Join-Path $desktopPath "AIFileProcessor_Architectural_Analysis.pdf"
Write-Host "PDF will be saved to: $pdfPath"

# Find Edge
$edgePaths = @(
  "${env:ProgramFiles}\Microsoft\Edge\Application\msedge.exe",
  "${env:ProgramFiles(x86)}\Microsoft\Edge\Application\msedge.exe"
)
$edgeExe = $edgePaths | Where-Object { Test-Path $_ } | Select-Object -First 1

if ($edgeExe) {
    Write-Host "Using Microsoft Edge: $edgeExe"
    # Convert HTML file URI
    $fileUri = "file:///$($htmlPath -replace '\\','/')"
    Write-Host "Source URI: $fileUri"
    
    # Run Edge headless and wait for it to complete
    $process = Start-Process -FilePath $edgeExe `
        -ArgumentList "--headless=new","--disable-gpu","--print-to-pdf=`"$pdfPath`"","--no-margins","$fileUri" `
        -Wait -PassThru -NoNewWindow
    
    Start-Sleep -Seconds 2
    
    if (Test-Path $pdfPath) {
        Write-Host "PDF created successfully at: $pdfPath"
        Remove-Item $htmlPath -Force
        Write-Host "Temporary HTML file cleaned up."
    } else {
        Write-Host "PDF was not created at the expected path. Trying alternative location..."
        $altPdfPath = Join-Path (Get-Location).Path "AIFileProcessor_Architectural_Analysis.pdf"
        $process = Start-Process -FilePath $edgeExe `
            -ArgumentList "--headless=new","--disable-gpu","--print-to-pdf=`"$altPdfPath`"","--no-margins","$fileUri" `
            -Wait -PassThru -NoNewWindow
        Start-Sleep -Seconds 2
        if (Test-Path $altPdfPath) {
            Write-Host "PDF created at alternative location: $altPdfPath"
            Remove-Item $htmlPath -Force
        } else {
            Write-Host "HTML file remains at: $htmlPath"
            Write-Host "Open it in your browser and use Print > Save as PDF."
        }
    }
} else {
    Write-Host "Microsoft Edge not found. HTML file saved to: $htmlPath"
    Write-Host "Open it in your browser and use Print > Save as PDF."
}