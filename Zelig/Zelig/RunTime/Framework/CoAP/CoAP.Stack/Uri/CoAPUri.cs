//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Stack
{
    using System;
    using System.Net;
    using System.Text;
    using CoAP.Common;

    public abstract class CoAPUri
    {
        //
        // coap URI Scheme: 
        //
        // coap-URI  = "coap:"  "//" host [ ":" port ] path-abempty [ "?" query ]
        // coaps-URI = "coaps:" "//" host [ ":" port ] path-abempty [ "?" query ]
        //

        public static readonly string Scheme__CoAP        = "coap://";
        public static readonly string Scheme__Secure_CoAP = "coaps://";

        public static readonly string WellKnown           = "/.well-known/";

        public static readonly char   PathSeparator       = '/';
        public static readonly char   PortSeparator       = ':';
        public static readonly char   QuerySeparator      = '?';
        public static readonly char   QueryDelimiter      = '&';
        public static readonly char[] UriSeparators       = new char[] { PathSeparator, PortSeparator, QuerySeparator };
        public static readonly char[] QueryDelimiters     = new char[] { QueryDelimiter };
        public static readonly char[] AllUriDelimiters    = new char[] { PathSeparator, PortSeparator, QuerySeparator, QueryDelimiter };

        public static readonly int    DefaultPort         = 5683;
        public static readonly int    DefaultSecurePort   = 5684;

        //--//

        //
        // State
        //

        private readonly string       m_scheme;
        private readonly IPEndPoint[] m_endPoints;
        private MessageOptions        m_options;
        private readonly string       m_path;

        //--//

        //
        // Constructors
        //

        protected CoAPUri( string scheme, IPEndPoint[ ] endPoint, string path )
        {
            m_options = new MessageOptions( );

            m_path = OptionsFromPath( path, ref m_options ); 

            m_scheme    = scheme;
            m_endPoints = endPoint;
        }

        protected CoAPUri( string scheme, IPEndPoint endPoint, string path ) : this( scheme, new IPEndPoint[] { endPoint }, path )
        {
        }

        //--//

        //
        // Helper methods
        //

        public override string ToString( )
        {
            //
            // use the first endpoint for a default address
            //

            return $"{m_scheme}{m_endPoints[0]}/{m_path}";
        }

        //--//
        private static string OptionsFromPath( string path, ref MessageOptions options )
        {
            var components = path.Split( UriSeparators );
            var fHasQuery  = path.IndexOf( QuerySeparator ) != -1;

            int numberOfComponents = components.Length;

            //
            // Must have a host name
            // 
            if(numberOfComponents < 1)
            {
                return path;
            }

            bool fSecondPath = false;
            var pathBuilder = new StringBuilder();
            for(int i = 0; i < numberOfComponents; ++i)
            {
                var value = components[ i ];

                if(String.IsNullOrEmpty( value ))
                {
                    continue;
                }

                if(fHasQuery && (i == numberOfComponents - 1))
                {
                    var queries = value.Split( QueryDelimiters );

                    for(int j = 0; j < queries.Length; ++j)
                    {
                        var query = queries[ j ];

                        options.Add(
                                new MessageOption_String( MessageOption.OptionNumber.Uri_Query, query )
                            );

                        var delimiter = (j == queries.Length - 1) ? '\0' : QueryDelimiter;

                        pathBuilder.Append( $"{query}{delimiter}" );
                    }
                }
                else
                {

                    options.Add(
                                new MessageOption_String( MessageOption.OptionNumber.Uri_Path, value )
                            );
                    
                    pathBuilder.Append( fSecondPath ? "/" + value : value );

                    fSecondPath = true;
                }
            }

            return pathBuilder.ToString( );
        }

        public static void UriStringToComponents( string uri, ref IPEndPoint destination, out string scheme, out string host, out int port, out string path, MessageOptions options )
        {
            scheme = null;
            host   = null;
            port   = 0;
            path   = null;

            int current  = 0;
            int length   = uri.Length;

            if(length < Scheme__CoAP.Length + 1)
            {
                throw new CoAP_UriFormatException( );
            }

            //
            // Identify the scheme first
            // 
            if(uri.StartsWith( Scheme__CoAP ))
            {
                scheme  = Scheme__CoAP;
                current = Scheme__CoAP.Length;
                port    = DefaultPort;
            }
            else if(uri.StartsWith( Scheme__Secure_CoAP ))
            {
                scheme  = Scheme__Secure_CoAP;
                current = Scheme__Secure_CoAP.Length;
                port    = DefaultSecurePort;
            }
            else
            {
                throw new CoAP_UriFormatException( );
            }

            //
            //  Find endpoint host and port, move forward until a ':' or a '/'
            //

            uri = uri.Substring( current, length - current );

            var components = uri.Split( UriSeparators );
            var fHasQuery  = uri.IndexOf( QuerySeparator ) != -1;
            var fHasPort   = uri.IndexOf( PortSeparator  ) != -1;

            int numberOfComponents = components.Length;

            //
            // Must have a host name
            // 
            if(numberOfComponents < 1)
            {
                throw new CoAP_UriFormatException( );
            }

            //
            // Resolve host name
            //
            var host1 = components[0];
            var ipAddress = Utils.AddressFromHostName( host1 );
            
            //
            // If there is a proposed destination (intermediary) and it does not match 
            // the host in the URI then we need to add a Uri-Host option.
            //
            if(destination != null && ipAddress.Equals( destination.Address ) == false)
            {
                options.Add(
                    new MessageOption_String( MessageOption.OptionNumber.Uri_Host, host1 )
                );

            }

            host = host1;

            //
            // Resolve port and Uri-Path/Uri-Query components
            // 
            if(numberOfComponents > 1)
            {
                int second = 1;
                int port1  = 0;

                //
                // Port
                //
                if(fHasPort)
                {
                    var portString = components[ second ];

                    if(String.IsNullOrEmpty( portString ))
                    {
                        ++second;
                    }
                    else
                    {
                        if(Int32.TryParse( components[ second ], out port1 ))
                        {
                            if(destination != null)
                            {
                                if(port1 != destination.Port && port1 != DefaultPort && port1 != DefaultSecurePort)
                                {
                                    options.Add(
                                        new MessageOption_Int( MessageOption.OptionNumber.Uri_Port, port1 )
                                    );
                                }
                            }

                            port = port1;

                            ++second;
                        }
                        else
                        {
                            throw new CoAP_UriFormatException( );
                        }
                    }
                }

                if(destination == null)
                {
                    destination = new IPEndPoint( ipAddress, port );
                }

                //
                // Uri-Path/Uri-Query
                //
                var pathBuilder = new StringBuilder();
                for(int i = second; i < numberOfComponents; ++i)
                {
                    var value = components[ i ];

                    if(String.IsNullOrEmpty( value ))
                    {
                        continue;
                    }

                    if(fHasQuery && (i == numberOfComponents - 1))
                    {
                        var queries = value.Split( QueryDelimiters );

                        for(int j = 0; j < queries.Length; ++j)
                        {
                            var query = queries[ j ];

                            options.Add(
                                    new MessageOption_String( MessageOption.OptionNumber.Uri_Query, query )
                                );

                            var delimiter = (j == queries.Length - 1) ? '\0' : QueryDelimiter;

                            pathBuilder.Append( $"{query}{delimiter}" );
                        }
                    }
                    else
                    {
                        options.Add(
                                    new MessageOption_String( MessageOption.OptionNumber.Uri_Path, value )
                                );

                        pathBuilder.Append( value + "/" );
                    }
                }

                path = pathBuilder.ToString( );
            }
        }
        
        public static IPEndPoint EndPointFromUri( string uri )
        {
            return EndPointFromUri( uri, DefaultPort ); 
        }

        public static IPEndPoint EndPointFromUri( string uri, int port )
        {
            //
            // TODO: Assumption is that uri is in the form 'coap[s]://<host>:<port>'
            // 
            if(uri.StartsWith( Scheme__CoAP ))
            {
                var schemeLength = Scheme__CoAP.Length;
                var portIndex    = uri.IndexOf( PortSeparator, schemeLength );

                if(portIndex != -1)
                {
                    var portEnd = uri.IndexOf( PathSeparator, portIndex );

                    port = Int32.Parse( uri.Substring( portIndex + 1, (portEnd == -1 ? uri.Length : portEnd) - portIndex - 1 ) );
                }
                else
                {
                    portIndex = uri.IndexOf( PathSeparator, schemeLength ); 

                    if(portIndex == -1)
                    {
                        portIndex = uri.Length;
                    }
                }

                var host = uri.Substring( schemeLength , portIndex - schemeLength );

                return new IPEndPoint( Utils.AddressFromHostName( host ), port );

            }
            else if(uri.StartsWith( Scheme__Secure_CoAP ))
            {
                throw new CoAP_UriFormatException( );
            }
            else
            {
                throw new CoAP_UriFormatException( );
            }
        }

        //
        // Access methods
        //

        public string Scheme
        {
            get
            {
                return m_scheme;
            }
        }

        public IPEndPoint[] EndPoints
        {
            get
            {
                return m_endPoints;
            }
        }

        public string Path
        {
            get
            {
                return m_path;
            }
        }

        public MessageOptions Options
        {
            get
            {
                return m_options;
            }
        }

        public bool IsSecure
        {
            get
            {
                return m_scheme.Equals( Scheme__Secure_CoAP );
            }
        }
    }
}
