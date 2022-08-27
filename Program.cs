using System.Net;
using System.Text;

class Program {
    static void send_string(HttpListenerResponse response, string responseString, System.IO.Stream output){
        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
        output.Write(buffer);
    }
    
    static void send_package(string package, HttpListenerResponse response, System.IO.Stream output){
        byte[] file = File.ReadAllBytes("packages/" + package + ".bin");
        string json = File.ReadAllText("packages/" + package + ".json");
        
        response.AddHeader("file", json);

        output.Write(file);        
    }
    static void post_package(string? package, HttpListenerRequest request){
        Stream data = request.InputStream;

        Console.WriteLine("Package created : " + package);

        string? json = request.Headers["file"];

        Stream filestream = File.Create("packages/" + package + ".bin");
        request.InputStream.CopyTo(filestream); 

        File.WriteAllText("packages/"+ package + ".json", json);

        filestream.Close();
    }
    static void update_package(){

    }
    static void listen(HttpListener listener){
        HttpListenerContext context = listener.GetContext();
        HttpListenerRequest request = context.Request;
        HttpListenerResponse response = context.Response;
        System.IO.Stream output = response.OutputStream;


        string message = "";
        string? package = request.Headers["name"]; 
        if(request.HttpMethod == "GET"){ // If the client is requesting something (thereby GET)
            message += "GET \n";
            if(package is not null){

                Console.WriteLine(request.Headers["type"]);
                Console.WriteLine("Package name : " + package);
                string full_path = Path.GetFullPath("packages/" + package);
                

                if(string.IsNullOrWhiteSpace(package) == true){ // If the client somehow sent no name or blank space as the package name
                    Console.WriteLine("No package declared");
                    message += "No package Declared\n";
                } else if(!File.Exists("packages/" + package + ".bin")){// If the client sent a package name but it doesn't exist
                    Console.WriteLine("Package " + package + "  does not exist!");
                    message += "Package " + package + " Does not exist!";
                }else if(!full_path.StartsWith("/Users/leonj/projects/bssh-host/packages/")){
                    Console.WriteLine("Cheeky?");
                    message += "What did you just try? :/";
                }else{          
                    if(request.Headers["type"] == "get-pkg"){ // If the type the client is asking for is get a package (What command they are asking off from)
                        send_package(package, response, output);
                    }
                }
            }else{
                message += "No package name sent";
            }
        }else if(request.HttpMethod == "POST"){
            Console.WriteLine("POST");
            message += "POST\n";
            post_package(package, request);
        }

        send_string(response, message, output);
        Console.WriteLine("\nText sent : " + message);
        output.Close();
    }
    public static void Main() {
        HttpListener listener = new HttpListener();

        listener.Prefixes.Add("http://*:8001/");

        listener.Start();
        int download = 0;
        while(true){
            Console.WriteLine("\n\nListening...                     (" + download + ")\n");
            listen(listener);
            download++;
        }
        //listener.Close();
        
    }

} // bssh get-pkg name
public class fileData{
    public List<int> bytes { get; set; } = new List<int>();

}