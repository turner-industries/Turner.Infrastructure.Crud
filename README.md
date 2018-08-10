## Introduction 
CRUD is intended to provide a package that complements the Turner Mediator by providing auto-generated request handlers for commonly-used (CRUD) requests.

E.g.
```
public class CreateUserRequest : ICrudCreateRequest<User, UserDto, UserGetDto>
{ 
    // Additional properties/configuration...
}
```

E.g.
```
// Directly from UsersController -> DELETE endpoint
return await HandleAsync(new CrudDeleteRequest<User>(x => x.Id));
```

## Dependencies
Turner.Infrastructure.Mediator
AutoMapper
SimpleInjector
EF Core

## Requests
* Create - Creates an entity and persists it
  * Does not check for uniqueness

* Get - Retrieves a **single** entity
  * Can use Filters
  * Does not check for existence

* Update - Retrieves entities, AutoMaps input data onto them, and persists the changes
  * Can use Filters
  * Does not check for existence

* Delete - Removes entities
  * Can use Filters
  * Does not check for existence

* Save - Attempts to update a **single** entity
  * If the entity does not exist, it will be created

* GetAll - Retrieves all entities
  * Can use Filters
  * Sortable

* Summarize - Retrieves all entities and maps to an array of `{Key, Property}`
  * Can use Filters
  * Sortable

* PagedGetAll - Retrieves a paged view of all entities
  * Can use Filters
  * Sortable

## Error Handling
When an error occurs, the library makes no assumption as to how the user should handle it.
Therefore, the plan is to not catch EF/.NET exceptions (allow them to bubble up), and to throw custom exceptions when any other error/failure occurs.
A Mediator decorator will be written to catch the exceptions, which will then be sent to a policy class along with the request's response for handling.
The default policy will simply rethrow the exceptions, but since the policies are injectable, the user will be free to modify this behavior.

## Filtering/Searching/Sorting
* "Filters" (as used above) refers to a collection of functions with the signature `bool (TEntity, TFilterValue)` or `bool (TEntity)`
  * E.g. `(entity, id) => entity.Id == id` or `entity => !entity.IsDeleted`
* "Sorters" (referred to by "Sortable") refers to a function with the signature: `IOrderedQueryable<TEntity> (IQueryable<TEntity>, TRequest)`
  * E.g. `(users, request) => users.OrderBy(x => x.EmailAddress)`
* Filter functions can be created automatically on request properties: E.g. `Filter(request => request.Id)`
  * Auto-generated filter functions may be conditional (the filter will be applied only if the property is not null): E.g. `FilterIf(request => request.TypeFilter)`
  * Auto-generated filter functions may 'search': E.g. `SearchIn(x => x.Name, StringComparison.InvariantCultureIgnoreCase)` or `SearchBeginning(x => x.Name)`
* Filter functions may be combined into conjunctions and disjunctions: E.g. `And(user => !user.IsDeleted, user => user.IsVerified)`
* Sorting functions can be created automatically on entity properties: E.g. `SortOn(user => user.EmailAddress)`