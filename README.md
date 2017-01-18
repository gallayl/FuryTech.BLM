# FuryTech.BLM
Business Layer Management utils for Entity Framework 6

## Concepts

The goal of the BLM packages is to create a separate layer for authorization, interpretation and event distributing logic between your data access layer (usually EF6) and your controller layer.

## Usage

### EfRepository
You can use the EfRepository<T> in the same way as you did with Entity Framework's DbSet<T>, but it will take care about handling the custom logic you've implemented in your authorizers / listeners / interpreters.
```cs
MyDbContext db = new MyDbContext();
EfRepository<MyEntityType> repository = new EfRepository<MyEntityType>(db);
// Get the entities you're authorized to see
var myAuthorizedEntities = repository.Entities(MyUserIdentity);
```
### ContextInfo
This is a custom class passed to the business logic functions with the following properties:
* IIdentity Identity - The user identity
* Task<IQueryable<T>> GetAuthorizedEntitySetAsync<T> - One specified authorized entity set
* IQueryable<T> GetFullEntitySet<T> - The unauthorized, full entity set (technically, EF's DbSet<T>)

### Authorizers
You can implement your own authorization for the entity types with them. The package supports these types of authorizers, you should derive from their abstracts:
#### AuthorizeCollection
Triggered, when a user gets the EfRepository's 'Entities' attribute
```cs
public class MyCollectionAuthorizer : AuthorizeCollection<MyEntity>
{
    public override async Task<IQueryable<MyEntity>> AuthorizeCollectionAsync(IQueryable<MyEntity>
    entities, IContextInfo ctx)
    {
        var myUser = ctx.Identity.GetMyUser();
        if (user.IsAdmin)
        {
            return entities;
        }

        return entities.Where(r => r.IsPublic);
    }
}
```
#### AuthorizeCreate
Triggered on each entity, when a user calls Repository.Add/AddAsync/AddRange/AddRangeAsync and new entities when calling Repository.SaveChanges/SaveChangesAsync.
```cs
public class CreateAdminOnly : IAuthorizeCreate<ICreateAdminOnly>
{
    public async Task<AuthorizationResult> CanCreateAsync(ICreateAdminOnly entity,
    IContextInfo ctx)
    {
        if (ctx.Identity.GetTerUser().IsAdmin)
        {
            return AuthorizationResult.Success();
        }

        return AuthorizationResult.Fail($"Only admin users are authorized to create entity with type '{entity.GetType().FullName}'", entity);
    }
}
```
#### AuthorizeModify
Triggered before modifying entity in the DB. Very similar to *AuthorizeCreate*, but the unchanged and the updated entity are both passed.

#### AuthorizeRemove
Triggered before deleting an entity from DB. Similar to *AuthorizeCreate*.

### Interpreters
They are to change a simple property value on the entity, before saving to the DB, you can eliminate DB triggers (BeforeInsert, BeforeUpdate) with them.
```cs
    public class CreatedByUserInterpreter : InterpretBeforeCreate<IHasCreator>
    {
        public override IHasCreator DoInterpret(IHasCreator entity, IContextInfo context)
        {
            var user = context.GetFullEntitySet<User>().FirstOrDefault(a => a.LoginName == context.Identity.Name);
            entity.CreatedByUser = user;
            return entity;
        }
    }

    public class ModifiedByInterpreter : InterpretBeforeModify<IHasModifiedBy>
    {
        public override IHasModifiedBy DoInterpret(IHasModifiedBy originalEntity, IHasModifiedBy modifiedEntity, IContextInfo context)
        {
            var modifier = context.GetFullEntitySet<User>().FirstOrDefault(a => a.LoginName == context.Identity.Name);
            modifiedEntity.ModifiedByUser = modifier;
            return modifiedEntity;
        }
    }
```

### EventListeners
...Docs coming soon. :)

### BLM Inheritance Principles

If you've implemented an authorizer/interpreter/event listener for a base class and you have a repository with the derived class, the authorizer/interpreter/listener will kick in when you use the derived repository as well. This works with interfaces also.

## NuGet links:

[FuryTechs.BLM.Base](https://www.nuget.org/packages/FuryTechs.BLM.Base)

[FuryTechs.BLM.EF6](https://www.nuget.org/packages/FuryTechs.BLM.EF6)
