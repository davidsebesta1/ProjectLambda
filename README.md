# Project Lambda

## Autor David Šebesta
### SPŠE Ječná
Project Lambda is a school project created to showcase work with database in C# and to show an example of non-repeatable read in databases transactions.

## Setup
In folder /data you can find a database export with basic login information "admin", "admin" (username, password) for the application interface to check out. There is also test data for January and Februrary of year 2025.<br>
Open MySQL and import the database. If needed, create new database login information for this database.<br>

## Usage
Program provides simple console-based menu to test the program.
There is a config.yaml provided/generated with the build.

Configuration options:
DatabaseIP - IP of the database to connect to (usually 127.0.0.1)<br>
LoginUser - Username of the login<br>
LoginPassword - Password for the db login<br>
InitialDatabase - LambdaDB (Do not change if you know what your doing)<br>
Available options for the isolation levels are:
- READ_UNCOMMITTED,
- READ_COMMITTED,
- REPEATABLE_READ,
- SERIALIZABLE

## Running the app
Run the .exe file or create your own linux build. Application is controlled by case-insensite command with their required parameters seperated by a single space.<br>
If at any time error occurs, check if you setup the database properly and if yes. Contact creator (me) via https://github.com/davidsebesta1/ProjectLambda/issues

## Used 3rd party libraries
https://github.com/BcryptNet/bcrypt.net
https://www.nuget.org/packages/MySql.Data/9.1.0#show-readme-container
https://github.com/aaubry/YamlDotNet

There is also fex external methods used in class Extensions - see to get the sources.

## Used architecture and patterns
Program is written in a way to be independed of where it is implemented.<br>
Program includes a simple console UI to showcase it usage, but the base code may be used everywhere else - like a webpage, GUI app, and such.<br>
This is because it is written in multitier architecture.<br>
It uses simple DAO pattern to retrieve objects from a database.<br>
Dao objects can be expanded with observer pattern to be fully compatible with standard MVVM architecture (for example for WinUI apps).<br>

## ER-diagram of the Database
![erDiagram](https://github.com/user-attachments/assets/9b12385d-f854-4921-b02b-127e9db6b5b0)

## Summary
Project Lambda is a simple console app to showcase working with the database.<br>
Although it is not perfect and still may be improved, there is an example of non-repeatale read which can a programmer use to see what happens when transactions race together.
