//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace CoAP.Stack
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;
    using CoAP.Common;

    public class MessageOptions : IEncodable, ICloneable
    {
        internal static readonly MessageOptions       EmptyOptions = new MessageOptions( );
        //internal static readonly MessageOption_Opaque EmptyETag    = new MessageOption_Opaque( MessageOption.OptionNumber.ETag, Constants.EmptyBuffer );

        //
        // State
        //

        private readonly LinkedList<MessageOption> m_allOptions;
        private          string                    m_uriPath; 
        private          List<string>              m_uriQueries;
        private          TimeSpan                  m_maxAge;
        private          MessageOption_Opaque      m_etag;

        //--//

        //
        // Contructors
        //

        public MessageOptions( )
        {
            m_allOptions = new LinkedList<MessageOption>( );
            m_uriQueries = new List<string>( ); 
            m_maxAge     = Defaults.MaxAge;
            m_etag       = null;
            //m_etag       = EmptyETag;
        }

        public object Clone( )
        {
            var options = new MessageOptions();

            foreach(var opt in this.m_allOptions)
            {
                options.Add( opt );
            }

            return options;
        }

        //
        // Helper Methods
        //
        
        public void Add( MessageOptions options )
        {
            if(options != null)
            {
                foreach(var option in options.Options)
                {
                    InsertInOrder( option );
                }
            }
        }

        public void Add( MessageOption option )
        {
            if(option != null)
            {
                InsertInOrder( option );
            }
        }

        private void InsertInOrder( MessageOption option )
        {
            //
            // Insert in order and preserve order of insertion
            // 
            LinkedListNode<MessageOption> node = m_allOptions.First;

            if(node == null)
            {
                m_allOptions.AddFirst( option );
            }
            else
            {
                do
                {
                    MessageOption current = node.Value;

                    if(option.Number < current.Number)
                    {
                        break;
                    }

                    //
                    // Throw on adding non-repeatable options, as it is illegal.
                    //
                    if(option.IsRepeatable == false && current.Number == option.Number)
                    {
                        throw new CoAP_MessageMalformedException( );
                    }

                    //
                    // Skip adding repeatable options with same value.
                    //
                    if(option.IsRepeatable && current.Equals( option ))
                    {
                        return;
                    }

                    if(node == m_allOptions.Last)
                    {
                        break;
                    }

                    node = node.Next;

                } while(true);
                

                if(option.Number < node.Value.Number)
                {
                    m_allOptions.AddBefore( node, option );
                }
                else
                {
                    m_allOptions.AddAfter( node, option );
                }
            }

            //
            // Update fast access links
            // 
            if(option.IsUriPath)
            {
                var path = (string)option.Value;

                m_uriPath += String.IsNullOrEmpty( m_uriPath ) ? path : "/" + path;
            }
            else if(option.IsUriQuery)
            {
                var query = (string)option.Value;

                m_uriQueries.Add( query );
            }
            else if(option.IsMaxAge)
            {
                m_maxAge = new TimeSpan( 0, 0, (int)option.Value );
            }
            else if(option.IsETag)
            {
                m_etag = (MessageOption_Opaque)option;
            }

            ValidateList( ); 
        }

        public bool Contains( MessageOption opt )
        {
            return m_allOptions.Find( opt ) != null; 
        }


        public void Encode( NetworkOrderBinaryStream stream )
        {
            if(m_allOptions.Count > 0)
            {
                var first = m_allOptions.First;

                //
                // initialize delta
                //
                first.Value.Delta = (int)first.Value.Number;

                for(var node = m_allOptions.First; node != null;)
                {
                    node.Value.Encode( stream );

                    var current = node;

                    node = node.Next;

                    //
                    // update delta
                    //
                    if(node != null)
                    {
                        node.Value.Delta = ((int)node.Value.Number - current.Value.Delta);
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        public override string ToString( )
        {
            var options = m_allOptions;

            var sb = new StringBuilder(); 

            for(var node = m_allOptions.First; node != null; node = node.Next)
            {
                sb.Append( node.Value ); 

                if(node != m_allOptions.Last)
                {
                    sb.Append( " + " ); 
                }
            }
            return $"[{sb}]";
        }
        
        //
        // Access Methods
        //
        
        public LinkedList<MessageOption> Options
        {
            get
            {
                return m_allOptions;
            }
        }

        public string Path
        {
            get
            {
                return m_uriPath;
            }
        }

        public string[ ] Queries
        {
            get
            {
                return m_uriQueries.ToArray( ); 
            }
        }

        public TimeSpan MaxAge
        {
            get
            {
                return m_maxAge;
            }
        }

        public MessageOption_Opaque ETag
        {
            get
            {
                return m_etag;
            }
            internal set
            {
                m_etag = value;
            }
        }

        public int Size
        {
            get
            {
                return ComputeOptionsLength( );
            }
        }

        //--//

        internal void UpdateDeltas( )
        {
            if(m_allOptions.Count > 0)
            {
                var first = m_allOptions.First;

                //
                // initialize delta
                //
                first.Value.Delta = (int)first.Value.Number;

                for(var node = m_allOptions.First; node != null;)
                {
                    var current = node;

                    node = node.Next;

                    //
                    // update delta
                    //
                    if(node != null)
                    {
                        node.Value.Delta = ((int)node.Value.Number - current.Value.Delta);
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        internal void Clear( )
        {
            m_allOptions.Clear( );

            m_uriPath = null;
            m_uriQueries.Clear( );
            m_maxAge = Defaults.MaxAge;
            m_etag = null;
    }

        private int ComputeOptionsLength( )
        {
            if(m_allOptions.Count == 0)
            {
                return 0;
            }

            int totalLength = m_allOptions.Count;

            for(var node = m_allOptions.First; node != null; node = node.Next)
            {
                var option = node.Value;
                var delta  = (int)option.Delta;

                int valueLength = option.ValueLength;

                totalLength += valueLength;
                
                if(delta > 12)
                {
                    //
                    // we need one or two additional bytes
                    //
                    int delta1 = delta - 13;

                    totalLength += (delta1 <= 255) ? 1 : 2;
                }
                
                if(valueLength > 12)
                {
                    //
                    // we need one or two additional bytes
                    //
                    int valueLength1 = valueLength - 13;

                    totalLength += (valueLength1 <= 255) ? 1 : 2;
                }
            }

            return totalLength;
        }

        //--//

        [Conditional( "DEBUG" )]
        private void ValidateList( )
        {
            var node = m_allOptions.First; 

            while(node != null)
            {
                if(node.Next != null && node.Next.Value.Number < node.Value.Number)
                {
                    Debug.Assert( false );
                }

                node = node.Next;
            }
        }
    }
}

