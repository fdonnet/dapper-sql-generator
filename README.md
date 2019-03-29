# MSSQL - Dapper generator
A simple and not too ambitious tool that helps you to generate some important generic layers for your C# project. It's extensible and modifiable. If you create your own generators an generator-settings and share your results via a pull request, we will be very happy to integrate it. Don't hesitate to enhance the quality of our coding, some parts are very "quick and dirty" stuff.

We dev this tool to help us in a real project. So it will help us for sure but we cannot guarantee it will help you ;).
## Goals
Use your Visual Studio Database Project (SSDT) to generate boring stuff to code. (good tables definition => good generation)

**Based on your tables definition and your generator configuration:**
- Generate standard stored procedures
- Generate base C# entities
- Generate base data access layer

## Limitations
A lot....... (but you can help!)
- Only works with SqlServer DB and ~Visual Studio Database Project. The generator uses a .dacpac file as the model entry point to eat the tables definition.
- You need to be on Windows to use the UI (WPF prj). Sorry for that guys. If you are on other systems, you can convert the logic layer and inject a JSON config file to the generator. It will work.
- The C# entities generator suits our needs. We let you see if it will suit yours.
- The C# repo generator targets **.netcore Dapper(async)** and it doesn't integrate a"repository" pattern. That's a choice and we will explain you why in more details in the repo section bellow... but you can change it or create a new generator for this part.

TO BE CONTINUED....
