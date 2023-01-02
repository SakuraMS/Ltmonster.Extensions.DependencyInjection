This is extensions of DependencyInjection.
Support service registration by adding interfaces or attribute.

## 1.Implementation ITransient interface
```csharp
public class MyService: ITransient
{
    public MyService()
    {
        
    }
}

public class MyService: IMyService, ITransient<IMyService>
{
    public MyService()
    {
        
    }
}

//The other two life cycle registration methods are supported
//ISingleton,ISingleton<T>,IScoped,IScoped<T>
```

## 2.Add Transient attribute
```csharp
[Transient]
public class MyService
{
    public MyService()
    {
        
    }
}

[Transient<IMyService>]
public class MyService: IMyService
{
    public MyService()
    {
        
    }
}

//The other two life cycle registration methods are supported
//[Singleton],[Singleton<T>],[Transient],[Transient<T>]
```

## 3.Multiple lifecycles
Of course, if you want to add services with multiple lifecycles, we support mixing and we do not double register.
```csharp
public class MyService: ITransient,ITransient<IMyService>
{
    public MyService()
    {
        
    }
}

[Transient<IMyService>]
public class MyService: IMyService,ITransient
{
    public MyService()
    {
        
    }
}
```

## 4.Used in Program.cs
```csharp
//1.Except that assemblies starting with "System." and "Microsoft." will be scanned.
builder.Service.AutoRegister();

//2.The assembly for configuration item "Ltmonster:ServiceAutoRegister:Assemblies" will be scanned,this way is recommended.
builder.Service.AutoRegister(builder.Configuration);
```
