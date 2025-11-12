# SellerTools

eCommerce Selling Tools

## Introduction

SellerTools is a collection of utilities designed to assist eCommerce sellers by automating, enhancing, and managing various sales processes and workflows. Built primarily with C#, it brings together robust automation, web scraping, and data management features for power sellers and marketplace managers.

## Features

- **Web Automation**: Automate browser actions with PuppeteerSharp and PuppeteerExtraSharp integrations.
- **Data Management**: SQLDBApp module for seamless database operations.
- **Web Applications**: Tools and dashboards through dedicated web app modules.
- **Flexible Architecture**: Designed for extension and tailored automation routines.

## Project Structure

- `Devweb/` - Development web tools and utilities.
- `PuppeteerSharp/` - Original PuppeteerSharp tooling for browser automation.
- `PuppeteerExtraSharp/` - Extended browser automation through PuppeteerExtraSharp.
- `SQLDBApp/` - Modules for interacting with SQL databases.
- `WebApp/` - Web application backend/frontend services.
- `.gitattributes` / `.gitignore` - Repository config files.
- `SellerTools.sln` - Visual Studio solution file.

## Installation

1. **Clone the repository**

   ```sh
   git clone https://github.com/yangnyc/SellerTools.git
   cd SellerTools
   ```

2. **Restore NuGet Packages**

   Open the solution in Visual Studio or run:

   ```sh
   dotnet restore
   ```

3. **Build the Solution**

   ```sh
   dotnet build
   ```

## Usage

### Web Automation

Utilize the modules in `PuppeteerSharp` and `PuppeteerExtraSharp` folders to run automated browser tasks, such as:
- Scraping online marketplace data.
- Auto-filling forms.
- Managing listings or inventory.

## Database Migration and Seeding

### Migration

**DataDB Migration:**
Database migration (drop and recreate) for development is handled via the `SQLDBApp/RecreateDatabase.cs` script. This utility will:

- Drop the `DataDB` database if it exists.
- Clean up any orphaned database files.
- Create a fresh `DataDB` database instance.

Update the connection string as required for your environment. Run the migration:

```sh
dotnet run --project SQLDBApp/RecreateDatabase.csproj
```
**AccountsDB Migration:**  
To create or migrate the `AccountsDB` database for development, follow a process similar to `DB2`, adjusting the database name and file paths in your script (copy `RecreateDatabase.cs` as a template). Make sure the connection string specifies `AccountsDB` instead of `DB2`, and that the file locations (for `.mdf` and `.ldf`) are correctly set.

See the template in `SQLDBApp/RecreateDatabase.cs` and adapt as needed for `AccountsDB`:

```csharp
var createCmd = new SqlCommand(@"
    CREATE DATABASE AccountsDB
    ON PRIMARY 
    (NAME = AccountsDB_Data, FILENAME = '/var/opt/mssql/data/AccountsDB_New.mdf')
    LOG ON 
    (NAME = AccountsDB_Log, FILENAME = '/var/opt/mssql/data/AccountsDB_New.ldf')
    ", connection);
```

You may implement your migration logic in a new file, e.g., `SQLDBApp/RecreateAccountsDB.cs`.

### Seeding

Populating the database with test data is performed by `SQLDBApp/SeedAllData.cs`. This script:

- Seeds thousands of category records and their relationships.
- Fills other related tables with random but realistic data.

Run the seeding script **after** migration:

```sh
dotnet run --project SQLDBApp/SeedAllData.csproj
```

These scripts are intended for development/testing purposes.  
For details, explore: [SQLDBApp/RecreateDatabase.cs](https://github.com/yangnyc/SellerTools/blob/master/SQLDBApp/RecreateDatabase.cs) and [SQLDBApp/SeedAllData.cs](https://github.com/yangnyc/SellerTools/blob/master/SQLDBApp/SeedAllData.cs)

---

For more utilities, browse the [SellerTools codebase](https://github.com/yangnyc/SellerTools/search?q=database).


### Database Operations

`SQLDBApp` includes utilities and example code for interacting with SQL databases relevant to your products, transactions, or reporting.

### WebApp Module

Host dashboards, analytics panels, or internal tools for use by sellers and their teams.

## Contribution

Pull requests are welcome! Please fork the repo and submit your changes for review.

## Credits and Thanks

This project would not be possible without the contributions and open-source efforts from:

- [sjdirect/abot](https://github.com/sjdirect/abot) – Original crawling framework. **Modified here and renamed to "Devweb"** as part of SellerTools development.
- [hardkoded/puppeteer-sharp](https://github.com/hardkoded/puppeteer-sharp) – .NET headless browser automation
- [Overmiind/Puppeteer-sharp-extra](https://github.com/Overmiind/Puppeteer-sharp-extra) – Extra plugins and automation features on top of Puppeteer-sharp

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

## Contact

For suggestions, contributions, or bug reports, please open an issue on the [GitHub repository](https://github.com/yangnyc/SellerTools).

---

_Developed and maintained by [yangnyc](https://github.com/yangnyc)_
