//#define PROD

using System.Runtime.CompilerServices;

#if !PROD
    [assembly: InternalsVisibleTo("Turner.Infrastructure.Crud.Tests")]
#endif