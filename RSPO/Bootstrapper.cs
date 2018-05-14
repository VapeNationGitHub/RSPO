using System;
using System.Collections.Generic;
using Nancy;
using SharpTAL;
using System.Linq.Expressions;
using System.IO;
using System.Reflection;
using Nancy.Conventions;
using Nancy.Responses;

namespace RSPO
{
    public class Bootstrapper : DefaultNancyBootstrapper
    {
        protected override void ConfigureConventions(NancyConventions nancyConventions)
        {
            string[] splitters = new string[]  {"static/"};
            base.ConfigureConventions(nancyConventions);

            Console.WriteLine("Initializing Bootstrapper:"+Application.STATIC_DIR);

            Conventions.StaticContentsConventions.Add(
                (context, rootPath) => {
                    var Request = context.Request;
                    string[] splitted = Request.Path.Split(splitters, System.StringSplitOptions.RemoveEmptyEntries);
                    if (splitted.Length !=2)
                    {
                        return null;
                    }
                    string[] restPath = splitted[1].Split('/');
                    string filePath = Path.Combine(restPath);
                    filePath = Path.Combine(Application.STATIC_DIR, filePath);
                    Console.WriteLine("READ:"+filePath);

                    var file = new FileStream(filePath, FileMode.Open);
                    string fileName = restPath[restPath.Length-1]; //set a filename

                    var response = new StreamResponse(() => file,
                                                      MimeTypes.GetMimeType(fileName));
                    return response;
                });
        }
    }
}
