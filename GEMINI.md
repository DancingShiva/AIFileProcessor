# Project Overview
This is a C# desktop application built on .NET 10 using Windows Forms. 

# Architecture & Components
* **UI Layer:** We utilize standard Windows Forms controls (primarily `RichTextBox`) for text rendering, data editing, and formatting.
* **Core Services:** The application orchestrates text tokenization and API integration. Core logic resides in classes like `ChineseTextProcessor` and the various AI services.

# Coding Standards
* Always adhere strictly to the C# specification.
* Ensure all nullable types (e.g., `int?` for timeouts) are handled correctly to avoid runtime null reference exceptions.
* **Text Processing Rules:** When working with the `ChinesePunctuationCleaner` or parsing reference strings, treat full-width Chinese punctuation as self-contained units. Do not mistake their internal whitespace for functional ASCII spaces.