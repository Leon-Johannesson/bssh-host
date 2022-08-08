using System.Net;
using System.Text;

class Program {
    static void send_string(HttpListenerResponse response, string responseString){
        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);

        // Get a response stream and write the response to it.
        response.ContentLength64 = buffer.Length;
        System.IO.Stream output = response.OutputStream;
        output.Write(buffer,0,buffer.Length);
        output.Close();
    }
    
    static void send_package(string package, HttpListenerResponse response){
        using( FileStream fs = File.OpenRead( @"C:\test\largefile.exe" ) ) {

            //response is HttpListenerContext.Response...
            response.ContentLength64 = fs.Length;
            response.SendChunked = false;
            response.ContentType = System.Net.Mime.MediaTypeNames.Application.Octet;
            response.AddHeader( "Content-disposition", "attachment; filename=largefile.EXE" );

            byte[] buffer = new byte[ 64 * 1024 ];
            int read;
            using( BinaryWriter bw = new BinaryWriter( response.OutputStream ) ) {
                while( ( read = fs.Read( buffer, 0, buffer.Length ) ) > 0 ) {
                    Thread.Sleep( 200 ); //take this out and it will not run
                    bw.Write( buffer, 0, read );
                    bw.Flush(); //seems to have no effect
                }

                bw.Close();
            }

        response.StatusCode = ( int )HttpStatusCode.OK;
        response.StatusDescription = "OK";
        response.OutputStream.Close();
        }
    }
    static void update_package(){

    }
    static void listen(HttpListener listener){
        HttpListenerContext context = listener.GetContext();
        HttpListenerRequest request = context.Request;
        HttpListenerResponse response = context.Response;

        string package;

        using (var reader = new StreamReader(request.InputStream, Encoding.UTF8)) {
            package = reader.ReadToEnd();
        }
        Console.WriteLine(request.Headers["type"]);
        
        string message = "";

        if(package == ""){
            Console.WriteLine("No package declared");
            message += "No package Declared\n";
        }

        if(request.HttpMethod == "GET"){
            message += "Get fked\n";
            if(request.Headers["type"] == "get-pkg"){
                send_package(package, response);
            }else if(request.Headers["type"] == "get-update"){

            }

        }else{
            Console.WriteLine("Post fked");
            message += "Post fked\n";
        }

        Console.WriteLine("Text sent : " + package);
        send_string(response, message);
    }
    public static void Main() {
        HttpListener listener = new HttpListener();

        listener.Prefixes.Add("http://*:8001/");

        listener.Start();
        
        while(true){
            Console.WriteLine("\n\nListening... ");
            listen(listener);
        }
    }

} // bssh get-pkg name
