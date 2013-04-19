using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Diagnostics;
using System.Net.Mime;
using System.IO;
using System.ComponentModel;

namespace JabbR.Eto.Interface.JabbR
{
    public class HttpServerRequestEventArgs : CancelEventArgs
    {
        public HttpListenerRequest Request { get; private set; }
        
        public HttpServerRequestEventArgs(HttpListenerRequest request)
        {
            this.Request = request;
        }
    }
    
    public class HttpServer : IDisposable
    {
        string baseDirectory;
        Dictionary<string, string> staticContent = new Dictionary<string, string>();
        
        public Dictionary<string, string> StaticContent
        {
            get { return staticContent; }
        }

        public Uri Url { get { return new Uri("http://" + "localhost" + ":" + port + "/"); } }

        HttpListener listener;
        int port = -1;
    
        public event EventHandler<HttpServerRequestEventArgs> ReceivedRequest;
        
        protected virtual void OnReceivedRequest(HttpServerRequestEventArgs e)
        {
            if (ReceivedRequest != null)
                ReceivedRequest(this, e);
        }

        public HttpServer(string baseDirectory)
        {
            this.baseDirectory = baseDirectory;
            var rnd = new Random();

            for (int i = 0; i < 100; i++)
            {
                int port = rnd.Next(49152, 65536);

                try
                {
                    listener = new HttpListener();
                    listener.Prefixes.Add("http://localhost:" + port + "/");
                    listener.Start();

                    this.port = port;
                    listener.BeginGetContext(ListenerCallback, null);
                    return;
                }
                catch (Exception x)
                {
                    listener.Close();
                    Debug.WriteLine("HttpListener.Start:\n" + x);
                }
            }

            throw new ApplicationException("Failed to start HttpListener");
        }

        void ListenerCallback(IAsyncResult ar)
        {
            if (!listener.IsListening)
                return;

            listener.BeginGetContext(ListenerCallback, null);

            var context = listener.EndGetContext(ar);
            var request = context.Request;
            var response = context.Response;
            
            var reqArgs = new HttpServerRequestEventArgs(request);
            OnReceivedRequest(reqArgs);
            
            if (reqArgs.Cancel)
            {
                response.Abort();
                return;
            }
            
            Debug.WriteLine("SERVER: " + baseDirectory + " " + request.Url);

            response.AddHeader("Cache-Control", "no-cache");

            try
            {
                string html;
                if (staticContent.TryGetValue(request.Url.AbsolutePath, out html))
                {
                    response.ContentType = MediaTypeNames.Text.Html;
                    response.ContentEncoding = Encoding.UTF8;

                    var buffer = Encoding.UTF8.GetBytes(html);
                    response.ContentLength64 = buffer.Length;
                    response.OutputStream.Write(buffer, 0, buffer.Length);
                    response.Close();
                    return;
                }

                
                string filePath = null;
                if (!string.IsNullOrEmpty(baseDirectory))
                    filePath = Path.Combine(
                        baseDirectory,
                        request.Url.AbsolutePath.Substring(1)
                    );
                
                if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                {
                    response.StatusCode = (int)HttpStatusCode.NotFound; // 404
                    response.StatusDescription = response.StatusCode + " Not Found";

                    response.ContentType = MediaTypeNames.Text.Html;
                    response.ContentEncoding = Encoding.UTF8;

                    var buffer = Encoding.UTF8.GetBytes("<html><body>404 Not Found</body></html>");
                    response.ContentLength64 = buffer.Length;
                    response.OutputStream.Write(buffer, 0, buffer.Length);

                    response.Close();
                    return;
                }

                byte[] entity = null;
                try
                {
                    entity = File.ReadAllBytes(filePath);
                }
                catch (Exception x)
                {
                    Debug.WriteLine("Exception reading file: " + filePath + "\n" + x);

                    response.StatusCode = (int)HttpStatusCode.InternalServerError; // 500
                    response.StatusDescription = response.StatusCode + " Internal Server Error";

                    response.ContentType = MediaTypeNames.Text.Html;
                    response.ContentEncoding = Encoding.UTF8;

                    var buffer = Encoding.UTF8.GetBytes("<html><body>500 Internal Server Error</body></html>");
                    response.ContentLength64 = buffer.Length;
                    response.OutputStream.Write(buffer, 0, buffer.Length);

                    response.Close();
                    return;
                }

                response.ContentLength64 = entity.Length;

                switch (Path.GetExtension(request.Url.AbsolutePath).ToLowerInvariant())
                {
                //images
                    case ".gif":
                        response.ContentType = MediaTypeNames.Image.Gif;
                        break;
                    case ".jpg":
                    case ".jpeg":
                        response.ContentType = MediaTypeNames.Image.Jpeg;
                        break;
                    case ".tiff":
                        response.ContentType = MediaTypeNames.Image.Tiff;
                        break;
                    case ".png":
                        response.ContentType = "image/png";
                        break;

                // application
                    case ".pdf":
                        response.ContentType = MediaTypeNames.Application.Pdf;
                        break;
                    case ".zip":
                        response.ContentType = MediaTypeNames.Application.Zip;
                        break;

                // text
                    case ".htm":
                    case ".html":
                        response.ContentType = MediaTypeNames.Text.Html;
                        break;
                    case ".txt":
                        response.ContentType = MediaTypeNames.Text.Plain;
                        break;
                    case ".xml":
                        response.ContentType = MediaTypeNames.Text.Xml;
                        break;

                // let the user agent work it out
                    default:
                        response.ContentType = MediaTypeNames.Application.Octet;
                        break;
                }

                response.OutputStream.Write(entity, 0, entity.Length);
                response.Close();
            }
            catch (Exception x)
            {
                Debug.WriteLine("Unexpected exception. Aborting...\n" + x);

                response.Abort();
            }
        }

        public void Dispose()
        {
            if (listener != null)
            {
                listener.Stop();
                listener.Close();
            }
        }
    }
}
