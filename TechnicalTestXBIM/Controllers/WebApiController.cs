using Newtonsoft.Json;
using System.IO;
using System.Web.Http;
using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;
using System.Linq;
using System.Collections.Generic;

namespace TechnicalTestXBIM.Controllers
{
    public class WebApiController: ApiController
    {

        // GET webapi/getinstances
        public string GetInstances()
        { 
            using (var model = IfcStore.Open(GetSampleFile()))
            {
                var walls = model.Instances.OfType<IIfcWall>().Count();
                var doors = model.Instances.OfType<IIfcDoor>().Count();
                var windows = model.Instances.OfType<IIfcWindow>().Count();

                var instances = new { Walls = walls, Doors = doors, Windows = windows };

                string json = JsonConvert.SerializeObject(instances, Formatting.Indented);

                return json;
            }
        }

        // GET webapi/getrooms
        public string GetRooms()
        {  
            using (var model = IfcStore.Open(GetSampleFile()))
            {
                //Get all spaces in the model. 
                //We use ToList() here to avoid multiple enumeration with Count() and foreach(){}
                var spaces = model.Instances.OfType<IIfcSpace>().ToList();
                var rooms = new List<object>();
                
                foreach (var space in spaces)
                {
                    //write report data
                    var room = new { RoomName = space.Name, Area = GetArea((IIfcSpace)space) };
                    rooms.Add(room);                    
                }

                var json = JsonConvert.SerializeObject(rooms, Formatting.Indented);
                return json;
            }
        }

        #region Private Methods

        private static IIfcValue GetArea(IIfcProduct product)
        {
            //try to get the value from quantities first
            var area =
                //get all relations which can define property and quantity sets
                product.IsDefinedBy

                //Search across all property and quantity sets. 
                //You might also want to search in a specific quantity set by name
                .SelectMany(r => r.RelatingPropertyDefinition.PropertySetDefinitions)

                //Only consider quantity sets in this case.
                .OfType<IIfcElementQuantity>()

                //Get all quantities from all quantity sets
                .SelectMany(qset => qset.Quantities)

                //We are only interested in areas 
                .OfType<IIfcQuantityArea>()

                //We will take the first one. There might obviously be more than one area properties
                //so you might want to check the name. But we will keep it simple for this example.
                .FirstOrDefault()?
                .AreaValue;

            if (area != null)
                return area;

            //try to get the value from properties
            return GetProperty(product, "Area");
        }

        private static IIfcValue GetProperty(IIfcProduct product, string name)
        {
            return
                //get all relations which can define property and quantity sets
                product.IsDefinedBy

                //Search across all property and quantity sets. You might also want to search in a specific property set
                .SelectMany(r => r.RelatingPropertyDefinition.PropertySetDefinitions)

                //Only consider property sets in this case.
                .OfType<IIfcPropertySet>()

                //Get all properties from all property sets
                .SelectMany(pset => pset.HasProperties)

                //lets only consider single value properties. There are also enumerated properties, 
                //table properties, reference properties, complex properties and other
                .OfType<IIfcPropertySingleValue>()

                //lets make the name comparison more fuzzy. This might not be the best practise
                .Where(p =>
                    string.Equals(p.Name, name, System.StringComparison.OrdinalIgnoreCase) ||
                    p.Name.ToString().ToLower().Contains(name.ToLower()))

                //only take the first. In reality you should handle this more carefully.
                .FirstOrDefault()?.NominalValue;
        }

        private static string GetSampleFile()
        {
            var sampleFile = Path.Combine(
                               Directory.GetCurrentDirectory(),
                               "wwwroot", "SampleHouse4.ifc");

            return sampleFile;
        }

        #endregion
    }
}
