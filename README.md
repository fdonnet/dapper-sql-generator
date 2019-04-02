# MSSQL - Dapper generator
A simple and not too ambitious tool that helps you to generate some important generic layers for your C# project (.netcore). 

It's extensible and modifiable. 

If you create your own generators an generator-settings, share your results via a pull request, we will be very happy to integrate it. Don't hesitate to enhance the quality of our coding, some parts are very "quick and dirty" stuff.

We dev this tool to help us in a real project. So it will help us for sure but we cannot guarantee it will help you...
## Goals
Use your Visual Studio Database Project (SSDT) to generate boring stuff to code. (good tables definition => good generation)

**Based on your tables definition and your generator configuration:**
- Generate standard stored procedures
- Generate base C# entities
- Generate base data access layer

## Limitations
A lot....... (but you can help!)
- Only works with SqlServer DB and ~Visual Studio Database Project. The generator uses a .dacpac file as the model entry point to eat the tables definition. (your tables and fields need to be in lowercase and with _ to separate words (table: user_role, field: has_access will be transformed in C# in PascalCase, UserRole and HasAccess)...
- You need to be on Windows to use the UI (WPF prj). Sorry for that guys. If you are on other systems, you can convert the logic layer and inject a JSON config file to the generator. It will work.
- The C# entities generator suits our needs. We let you see if it will suit yours.
- The C# repo generator targets **.netcore Dapper(async)** (ex: DAL layer for webapi netcore project) and it doesn't integrate a"repository" pattern. That's a choice and we will explain you why in more details in the repo section bellow... but you can change it or create a new generator for this part.

## Generate
![Alt text](/img/output.png?raw=true "Generate output")
- For TSQL stored procedures, select a folder in your Visual Studio Database tool (SSDT). You will be able to use the "compare tool" to update your database with the generated SPs.
- For C# entities, select the folder in your target project where are located your entities. (include the generated file in your Visual Studio solution)
- For C# repo, select a folder where is located your target DAL layer. (include the generated file in your Visual Studio solution)

## Generators (settings)
First step (load your .dacpac file)
![Alt text](/img/load.PNG?raw=true "Load your model")

After that, you can define your settings at a global level (all tables) or override for each table level in the "Table generation settings" (right pan, near preview tab):
![Alt text](/img/settings.png?raw=true "Settings")

Some specific settings are only available at the "Table level". 
We let you discover !

When you are happy with your model settings, don't forget to save your config:
![Alt text](/img/save_config.png?raw=true "Save your config")

It will save all the defined settings in a JSON file and you will be able to load it when your base model has been changed. (ex: new settings for new tables without loosing your old configuration) 
