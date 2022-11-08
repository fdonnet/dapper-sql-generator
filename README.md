For MySql or to see the new version i m working on, go => https://github.com/fdonnet/Dapper-Layers-Generator (Console app, multi db provider) 

# MSSQL - Dapper generator
A simple and not too ambitious tool that helps you to generate some important generic layers for your MSSQL - C# (netcore) - Dapper project. 

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
- The C# repo generator targets **.netcore Dapper(async)** (ex: DAL layer for webapi netcore project) and it doesn't integrate a"repository" pattern. That's a choice... but you can change it or create a new generator for this part.
- No "get entity with children" function (maybe if we will have more time, or if someone want to help). We know how to do it but it's not a small job to automate it.

## Generate
![Alt text](/img/output.png?raw=true "Generate output")
- For TSQL stored procedures, select a folder in your Visual Studio Database tool (SSDT) project. You will be able to use the "compare tool" to update your database with the generated SPs.
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

## TSQL stored procedures generator
In "General" tab, you can select what types of SP you want o generate.

At global and table level you can specify:

![Alt text](/img/sqlsettings.png?raw=true "Stored proc settings")
- The roles you want to grant "EXECUTE" on the SP (the roles need to be defined in your model .dacpac file).
- For some SP types, you have the possibility to directly exclude some fields.
- For "select by UK" generator, you need to specify unique constrain(s) in your sql table definition.

## C# Entities generator
In the "General" tab:

![Alt text](/img/entitiesglobal.png?raw=true "Entities settings")

*(All this settings, can be defined at the global level or can be overrided at the table lvl config)*

- Define a namespace 
- Implement interface(s) in the generated entity
- Implement ICloneable (with a minimal .Clone() template function)
- Set some standards decorators

At the table lvl settings, you can:

- Define a custom type (ex: if you have enum fields) or custom decorator for each table column:
![Alt text](/img/entitiesfieldtype.png?raw=true "Field type settings") ![Alt text](/img/entitiescustomdeco.png?raw=true "Decorators settings")

## C# Repo/DAL generator
It will generate ONE simple DBContext (interface + class) and a repo/dal class for each table (+interface).

The repo/dal for each table **will be defined as "partial"**. => You will be able to extend your functionnalities outside of the generated files (via a new file for the interface or the class definition).

In the settings, you can define the connection string **name** that will be injected via netcore IConfiguration object in the DbContext settings:

![Alt text](/img/repoconstring.png?raw=true "Connection name")

! **It's the connection name, not the connection string...** !

DbContext constructor:

```csharp
public DbContext(IConfiguration config)
 {
     _config = config;
     DefaultTypeMap.MatchNamesWithUnderscores = true;
     _cn = new SqlConnection(_config.GetConnectionString("Default"));
 }
```
 
### No Repository pattern => only a simple DAL you can extend

*Biased approach... maybe it will not fit your needs....*

Only available, for the moment" with Dapper async implementation...

In netcore, see some implementations bellow if you have a service layer, and an api layer (controller):

- Inject your DbContext in the service constructor (**tips: you can choose the name you want in the generator for your DbContext class**) . 

*You can see we have injected the IConfiguration too...*

```csharp
 private readonly ILogger<ServiceTest> _log;
 private readonly IConfiguration _config;
 private readonly IDbContext _context;
 
 public ServiceTest(ILogger<ApplicationManager> logger, IConfiguration config, 
    IDbContext context)
 {
     _log = logger;
     _config = config;
     _context = context;
 }
```

In your controller, inject your service ServiceTest:
```csharp
private readonly ILogger<MyController> _log;
private readonly IConfiguration _config;
private readonly IServiceTest _serviceTest;

public MyController(ILogger<MyController> logger, IConfiguration config,
    IServiceTest serviceTest)

{
    _log = logger;
    _config = config;
    _serviceTest = serviceTest;
}
```

In your netcore startup / configure services function:
- Add your dbcontext as transient/scoped
- And your services as transient/scoped
```csharp
services.AddTransient<IDbContext, DbContext>();

services.AddScoped<IServiceTest, ServiceTest>();
```

netcore, so good! Your DbContext will be injected automatically via your service injection in the controller.
You will be able to use your DAL in your service layer (the DbContext will be able to call all the repo/dal you have defined).

Reminder: You can extend your interface and repo class via another "partial" file. It allows you to re-generate your base definition via the tool anytime.

#### And if I need a transaction between repos ?
From a method of your service/core layer:

The using statement is not really needed but it protects you from yourself;
```csharp
//Open a transaction
 using(var trans = await _dbContext.OpenTransaction())
 {
    //Execute operations example
    var resultDel = await _dbContext.PermissionRepo.DeleteRoles(permissionId);
    bool ok = await _dbContext.RolePermissionRepo.InsertByLinkedIds(linkedIds, currentUserId, DateTime.Now);

    //Commit transaction
    _dbContext.CommitTransaction();
 }
```

Async transaction is a big debate. We choose to not use the .net transaction (TransactionScopeAsyncFlowOption) because we dont' really understand how it is working behind. We prefer to use a standard IDBTransaction, because we think it's finally not too bad... but you could convince us...

#### And if I need more control on my DBContext lifecycle ?
In case of parallel jobs (Tasks) or if you use the DAL in a NON Web/Web Api scope... the lifetime of our Dbcontexts needs to be more managed.
The generator implements a very minimalistic DBContextFactory. So you can inject the IDBContextFactory in your services constructor and keep the control :
```csharp
//Parallel 2 tasks jobs (thread safe)
Task<IEnumerable<Object1>> object1Task;
Task<IEnumerable<Object2>> object2Task;

//Job1
using var dbContext1 = _dbContextFactory.Create();
object1Task = dbContext1.object1Repo.GetAll();

//Job2
using var dbContext2 = _dbContextFactory.Create();
object2Task = dbContext2.object2Repo.GetAll();

//Wait for jobs finished and retrieve the result
// Don't close the Dbcontexts before Task.WhenAll
await Task.WhenAll(object1Task, object2Task);
var object1List = await object1Task;
var object2List = await object2Task;
```
See above : 2 separate DB contexts to open 2 parallel db connections..



