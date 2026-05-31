# Game page refactoring
- remove PlayerRoundState from PlayerGameBoard
- PlayerGameBoard should expose only AvailableTiles and SelectedTiles 
- PlayerGameBoard should have only OnSubmit event which handles in page based on game rules