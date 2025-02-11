# Issues

# Wishlist
- Support packaging the app

# Changes

## 07/12/2024
PortfolioManager application: supports the following functionality:
- Flyout menu with page selection using tags.
- Settings page with settings card
Plus:
- added application icons
- added support for sqlite
- added 'barebones' PortfolioItem to the model
- add sqlitedataservice and interface
- added FolderSelectorService (and interface) to support setting the database folder
- added settings card for setting the database folder

## 09/12/2024
- added support for setting the python library (essential in order to initialise pythonnet).
The support is divided into two areas of responsibility:
1) We define a PythonLibrarySelectorService (derived from IFileSelectorService) that communicates with the LocalSettingsService to manage saving and retrieving this specific setting.
2) In the SettingsViewModel, we declare an instance of PythonLibrarySelectorService and a command called from the SettingsView to select the python library file and save it to settings.

## 13/01/2025 - 15/01/2025
- Removed async data processing. 
- Sort out issue initialising the data filename -> need to add code to the activation service.
- PythonService: use PyIOStream etc -> required for redirecting console output from python
- Clean up unused artifacts from Community Toolkit template
- Added support for getting the portfolio analysis params (benchmark, start and end date and risk free rate).
- Update PortfolioAnalysisParamsDialog to properly set the state of the primary button
- Refactor the interop layer
- Add bool flag and store exception message in the portfolio item: this allows reporting via ReportException()
- Add support for benchmark data and plots

## 16/01/2025
Initial checkin.

## 07/02/2025
- Add support for an sqlite database instead of csv file.
- Enhance database support to include add, edit, delete operations
- Refactor the UI to add portfolio item details and ticker values




