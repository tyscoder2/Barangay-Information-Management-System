Instructions:
To download, click the green "<> Code" button and "Download Zip". Extract and open with Visual Studio.

Before running:
1) Setup the internal database: make sure Microsoft.Data.SqlClient is installed for the project. **Also note: if BaranggayInformationSystem database exists before this, delete or rename it to something else!**
2) Create the database if not yet already created.
* View -> SQL Server Object
* Then below SQL Server, right-click on "(localdb)\MSSQLLocalDB" and "New Query..."
* Copy the code from DBCreationComplete.sql and paste in the new query and run with green play button at the upper left.

Note: if this one doesn't work, try running the code one-by-one, in order:
* 1_DBCreation.sql
* 2_DBTablesCreation.sql
* 3_DBCreationFinalAdd.sql
* 4_DBLogicalDeletionFullCrudAddition.sql

[Video for reference]([https://drive.google.com/drive/folders/1QbrMHMEj4fN2YEFOq7n-kXQ-3s78e5T_?dmr=1&ec=wgc-drive-%5Bmodule%5D-goto](https://drive.google.com/drive/folders/1vsxiKXc55mg4oABFCQWtEcZR_BKV0iBS?dmr=1&ec=wgc-drive-%5Bmodule%5D-goto))

3) Once all code runs without errors, open the project and run. On opening, you will be prompted to make an Admin account.

[Demonstration videos here of the program's functions.](https://drive.google.com/drive/folders/1QbrMHMEj4fN2YEFOq7n-kXQ-3s78e5T_?dmr=1&ec=wgc-drive-%5Bmodule%5D-goto)

Some notes:
-Some database edits were made (such as merging complaints and blotters into case files).
-CRUD is implemented, but deletion makes use of logical deleting rather than hard deleting.

A compiled version exists here (WIP).
