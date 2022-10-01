using System.Net;
using BC = BCrypt.Net.BCrypt;

class Program {
    static HttpListenerContext context;
    static HttpListenerRequest request;
    static HttpListenerResponse response;
    static System.IO.Stream output;
    static void send_string(string responseString){
        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
        output.Write(buffer);
    }
    static void send_package(string package){
        // Pretty simple sends a package by taking all the bytes of the package
        // sending the bytes with a instruction on how to divide them correctly

        byte[] file = File.ReadAllBytes("packages/" + package + ".bin");
        string json = File.ReadAllText("packages/" + package + ".json");
        
        response.AddHeader("file", json);

        output.Write(file);        
        Console.WriteLine("Sent package " + package);
    }
    static void post_package(string package){
        // wip  Will allow users to post their own packages

        Stream data = request.InputStream;

        Console.WriteLine("Package created : " + package);

        string? json = request.Headers["file"];

        Stream filestream = File.Create("packages/" + package + ".bin");
        request.InputStream.CopyTo(filestream); 

        File.WriteAllText("packages/"+ package + ".json", json);

        filestream.Close();

        Console.WriteLine("Posted package " + package);
    }
    static string get_pkg(string package){
        // This does some of the raw checking to see that nothing is wrong with the 
        // Package that the user is requesting (for example the user doing something like ..
        // Which would allow them to download random files on the server)
        // Or somehow sending blankspaces or just simply a package that doesnt exist


        Console.WriteLine(request.Headers["type"]);
        Console.WriteLine("Package name : " + package);
        string full_path = Path.GetFullPath("packages/" + package);

        if(string.IsNullOrWhiteSpace(package)){ // If the client somehow sent no name or blank space as the package name
            Console.WriteLine("No package declared");
            return "No package Declared";
        }
        if(!File.Exists("packages/" + package + ".bin")){// If the client sent a package name but it doesn't exist
            Console.WriteLine("Package " + package + "  does not exist!");
            return "Package " + package + " does not exist!";
        }
        if(!full_path.StartsWith("/Users/leonj/projects/bssh-host/packages/")){ // If the user has somehow tried to access different locations
            Console.WriteLine("Cheeky?");
            return "Weird package name";
        }         
        if(request.Headers["type"] == "get-pkg"){ // If the type the client is asking for is get a package (What command they are asking off from)
            send_package(package);
            return "Package sent";
        }
        return "";
    }
    static void closeOutput(string message){
        // One of the last things is sending a message with everything that happened and
        // Then closing streams as to not make any lingering connections

        send_string(message);
        Console.WriteLine("\nText sent : " + message);
        output.Close();
    }
    static bool pass(){
        string? pass = request.Headers["pass"];

        if(string.IsNullOrWhiteSpace(pass)){
            return false;
        }
        string[] lines = File.ReadAllLines("password.txt");  
        foreach (string line in lines){ 
            if(BC.Verify(pass, line)){
                return true;
            }
        }
        return false;
    }
    static void listen(HttpListener listener){
        // Sets up the program to whatever user is requesting a function
        // Figures out what the user wants and sends them on their way :)

        context = listener.GetContext();
        request = context.Request;
        response = context.Response;
        output = response.OutputStream;
        
        if(!pass()){ return;}

        string? package = request.Headers["name"]; 
        if(package == null){
            closeOutput("No package name sent");
            return;
        }
        if(request.HttpMethod == "GET"){ // If the client is requesting something (thereby GET)
            string message = get_pkg(package);
            closeOutput(message);
            return;
        }
        if(request.HttpMethod == "POST"){
            post_package(package);
            string message = "Package " + package + " posted";
            closeOutput(message);
            return;
        }
        output.Close();
        return;
    }
    public static void Main() {
        HttpListener listener = new HttpListener();

        listener.Prefixes.Add("http://*:8001/");

        listener.Start();
        int download = 1;
        while(true){
            Console.WriteLine("\n\nListening...                     (" + download + ")\n");
            listen(listener);
            download++;
        }
        //listener.Close();
        
    }
} // bssh get-pkg name