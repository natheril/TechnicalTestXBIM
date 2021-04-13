# TechnicalTestXBIM

All that needs to be done is open the solution in Visual Studio, and run it. Send requests to the localhost to get information via the web api (e.g. GET ../webapi/getinstances)

The structure of the code is straight-forward:

  1). WebApiController - this controls incoming GET requests and responds with the information requested in the for of a JSON output
    a). GET ../webapi/getinstances - this will return information of three types of elements (doors, windows and walls) along with the number of instances within the model
    b). GET ../webapi/getrooms - this will return information on the number of rooms within the model as well as information of the area size
    
  2). HomeController - this controls the web pages, as well as the Upload Files function, which you can use to upload any number of files (anything other than IFC files will 
                       result in an error)
                       
  3). A couple of Models for viewing the information on the files uploaded as well as some extensions for the IFormFile
  
  4). Two View files for the visual aspect of the pages (the Home page with the upload function, and the Files page containing a list of the files uploaded)
  
  5). Some extra routes in the Startup.cs file for the two controllers
  
  


