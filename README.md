# SellerTools

_eCommerce Selling Tools_

## Overview

SellerTools is a toolkit designed for online sellers and eCommerce platforms. It provides automation, scraping, and optimization functionalities to streamline product management on various marketplaces. Built primarily in C#, with enhancements using SCSS, JavaScript, CSS, and HTML.

## Features

- Automated browser interaction and scraping for marketplace management
- UI components and dashboards for seller analytics
- Customizable tools for different eCommerce workflows
- Modular architecture for extending and adapting new features

## Getting Started

1. **Prerequisites**
   - [.NET 6.0+](https://dotnet.microsoft.com/download)
   - Node.js (for building web assets, if applicable)
2. **Installation**
   ```bash
   git clone https://github.com/yangnyc/SellerTools.git
   cd SellerTools
   # Restore dependencies and build (for .NET):
   dotnet restore
   dotnet build
   # (For frontend/scss/js assets):
   npm install
   npm run build
   ```
3. **Usage**
   - See `docs/` folder or application-specific docs for usage guidance.
   - Run with:
     ```bash
     dotnet run
     ```

## Credits and Thanks

This project would not be possible without the contributions and open-source efforts from:

- [hardkoded/puppeteer-sharp](https://github.com/hardkoded/puppeteer-sharp) – .NET headless browser automation
- [Overmiind/Puppeteer-sharp-extra](https://github.com/Overmiind/Puppeteer-sharp-extra) – Extra plugins and automation features on top of Puppeteer-sharp
- [sjdirect/abot](https://github.com/sjdirect/abot) – Original crawling framework. **Modified here and renamed to "Devweb"** as part of SellerTools development.

Special thanks to the maintainers and contributors of these projects!

## License

SellerTools is released under the MIT License.

```
MIT License

Copyright (c) 2025 yangnyc

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```

## Contributing

Pull requests and issues are welcome!  
See [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

## Language Composition

- **C#** (72.6%)
- **SCSS** (12.8%)
- **JavaScript** (6.2%)
- **CSS** (5.9%)
- **HTML** (2.5%)

---

_Developed and maintained by [yangnyc](https://github.com/yangnyc)_
