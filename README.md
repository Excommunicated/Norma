Norma
=====
A modular approach to auditing your ORM. Current projects allow auditing of an EF 6.1.2 model and storing in a SQL Server database.

Installation
=====
Download the source and build the project. Add references to the `Norma.Core`, `Norma.EF` and `Norma.SqlServer` dll's in your project.

After install, update your existing [OWIN Startup](http://www.asp.net/aspnet/overview/owin-and-katana/owin-startup-class-detection) file with the following lines of code.

```csharp
app.UseNorma(config =>
{
  config.UseSqlServerStorage("<connection string or it's name>");
  config.UseEntityFrameworkDbInterceptor();
});
```
If you are not using OWIN, or an ASP.Net application, you can also use the following code.

```csharp
AuditLogStorage.Current = new SqlServerStorage("<connection string or it's name>");
DbInterception.Add(new EfAuditLogInterceptor);
```
Add the `[Auditable]` attribute to your model classes that you want to capture.

Roadmap
======
- Add appropriate commenting in code to attribute credit to the originator.
- Testing, Testing and more Testing.
- Finish Fluent interface for mapping audit classes and properties.
- Implement Other ORM's (e.g. [Nhibernate](http://nhibernate.info/)).
- Finish enough to create Nuget packages for easier install and update.
- Documentation.
- Everything else I've forgotten.

Credits
=======
Norma uses code, design, ideas and inspiration from the following projects:-
- [Hangfire](http://hangfire.io) by [@odinserj](https://github.com/odinserj)

   General Design concept, Taken heavily from Core, Most of the SqlServer storage is a direct copy.

- [EF.Audit](https://github.com/biasmey/EF.Audit) by [@biasmey](https://github.com/biasmey)

   Idea for the project, Use of attributes to markup entities. Found some non functioning code to implement the `IDbCommandInterceptor, IDbCommandTreeInterceptor` interfaces that was cleaned up and made functional.

- [EntityFramework](http://entityframework.codeplex.com/) by [@microsoft](http://microsoft.com)

   Ideas taken from the DbModelBuilder interface to allow a fluent approach to marking up entities to be audited. designed to replace the need for the attribute model (Not yet functional)
