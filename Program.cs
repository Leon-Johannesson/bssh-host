using System.Net;
using System.Text;

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
        byte[] file = File.ReadAllBytes("packages/" + package + ".bin");
        string json = File.ReadAllText("packages/" + package + ".json");
        
        response.AddHeader("file", json);

        output.Write(file);        
        Console.WriteLine("Sent package " + package);
    }
    static void post_package(string package){
        Stream data = request.InputStream;

        Console.WriteLine("Package created : " + package);

        string? json = request.Headers["file"];

        Stream filestream = File.Create("packages/" + package + ".bin");
        request.InputStream.CopyTo(filestream); 

        File.WriteAllText("packages/"+ package + ".json", json);

        filestream.Close();

        Console.WriteLine("Posted package " + package);
    }
    static void update_package(){

    }
    static string get_pkg(string package){

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
        send_string(message);
        Console.WriteLine("\nText sent : " + message);
        output.Close();
    }
    static void listen(HttpListener listener){
        context = listener.GetContext();
        request = context.Request;
        response = context.Response;
        output = response.OutputStream;


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