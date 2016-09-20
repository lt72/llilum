//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace System.Net
{
    using Runtime.CompilerServices;
    using System.Net.Sockets;

    /// <devdoc>
    ///    <para>Provides an internet protocol (IP) address.</para>
    /// </devdoc>
    [Serializable]
    public class IPAddress
    {
        public static readonly IPAddress Any = new IPAddress(0x0000000000000000);
        public static readonly  IPAddress Loopback = new IPAddress(0x000000000100007F);
        public static readonly  IPAddress Broadcast = new IPAddress(0x00000000FFFFFFFF);
        public static readonly  IPAddress None = Broadcast;

        internal const long LoopbackMask = 0x00000000000000FF;

        //--//

        internal long m_Address;

        public IPAddress(long newAddress)
        {
            if (newAddress < 0 || newAddress > 0x00000000FFFFFFFF)
            {
                // BUG: This always throws. Needs investigation
                //throw new ArgumentOutOfRangeException();
            }

            m_Address = newAddress;
        }

        public IPAddress(byte[] newAddressBytes)
            : this(((((newAddressBytes[3] << 0x18) | (newAddressBytes[2] << 0x10)) | (newAddressBytes[1] << 0x08)) | newAddressBytes[0]) & ((long)0xFFFFFFFF))
        {
        }

        public override bool Equals(object obj)
        {
            IPAddress addr = obj as IPAddress;

            if (obj == null)
                return false;

            return this.m_Address == addr.m_Address;
        }

        public override int GetHashCode()
        {
            return (int)this.m_Address;
        }

        public byte[] GetAddressBytes()
        {
            return new byte[]
            {
                (byte)(m_Address),
                (byte)(m_Address >> 8),
                (byte)(m_Address >> 16),
                (byte)(m_Address >> 24)
            };
        }

        public AddressFamily AddressFamily
        {
            get
            {
                return AddressFamily.InterNetwork;
            }
        }
        
        public static bool TryParse( string ipString, out IPAddress address )
        {
            address = Parse( ipString );
            return address != null;
        }

        public static IPAddress Parse( string ipString )
        {
            if (ipString == null)
                throw new ArgumentNullException();

            ulong ipAddress = 0L;
            int lastIndex = 0;
            int shiftIndex = 0;
            ulong mask = 0x00000000000000FF;
            ulong octet = 0L;
            int length = ipString.Length;

            for (int i = 0; i < length; ++i)
            {
                // Parse to '.' or end of IP address
                if (ipString[i] == '.' || i == length - 1)
                    // If the IP starts with a '.'
                    // or a segment is longer than 3 characters or shiftIndex > last bit position throw.
                    if (i == 0 || i - lastIndex > 3 || shiftIndex > 24)
                    {
                        throw new ArgumentException();
                    }
                    else
                    {
                        i = i == length - 1 ? ++i : i;
                        octet = (ulong)(ConvertStringToInt32(ipString.Substring(lastIndex, i - lastIndex)) & 0x00000000000000FF);
                        ipAddress = ipAddress + (ulong)((octet << shiftIndex) & mask);
                        lastIndex = i + 1;
                        shiftIndex = shiftIndex + 8;
                        mask = (mask << 8);
                    }
            }

            return new IPAddress((long)ipAddress);
        }

        public override string ToString()
        {
            return ((byte)(m_Address)).ToString() +
                    "." +
                    ((byte)(m_Address >> 8)).ToString() +
                    "." +
                    ((byte)(m_Address >> 16)).ToString() +
                    "." +
                    ((byte)(m_Address >> 24)).ToString();
        }

        public static bool IsLoopback( IPAddress address )
        {
            if(address == null)
            {
                throw new ArgumentNullException( "address" );
            }
            //////if(address.m_Family == AddressFamily.InterNetworkV6)
            //////{
            //////    //
            //////    // Do Equals test for IPv6 addresses
            //////    //
            //////    return address.Equals( IPv6Loopback );
            //////}
            //////else
            //////{
                return ((address.m_Address & IPAddress.LoopbackMask) == (IPAddress.Loopback.m_Address & IPAddress.LoopbackMask));
            //////}
        }

        internal bool IsBroadcast
        {
            get
            {
                //////if(m_Family == AddressFamily.InterNetworkV6)
                //////{
                //////    //
                //////    // No such thing as a broadcast address for IPv6
                //////    //
                //////    return false;
                //////}
                //////else
                //////{
                    return m_Address == Broadcast.m_Address;
                //////}
            }
        }

        //--//
        ////////////////////////////////////////////////////////////////////////////////////////
        // this method ToInt32 is part of teh Convert class which we will bring over later
        // at that time we will get rid of this code
        //

        /// <summary>
        /// Converts the specified System.String representation of a number to an equivalent
        /// 32-bit signed integer.
        /// </summary>
        /// <param name="value">A System.String containing a number to convert.</param>
        /// <returns>
        /// A 32-bit signed integer equivalent to the value of value.-or- Zero if value
        /// is null.
        /// </returns>
        /// <exception cref="System.OverflowException">
        /// Value represents a number less than System.Int32.MinValue or greater than
        /// System.Int32.MaxValue.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// The value parameter is null.
        /// </exception>
        /// <exception cref="System.FormatException">
        /// Value does not consist of an optional sign followed by a sequence of digits
        /// (zero through nine).
        /// </exception>
        private static int ConvertStringToInt32(string value)
        {
            char[] num = value.ToCharArray();
            int result = 0;

            bool isNegative = false;
            int signIndex = 0;

            if (num[0] == '-')
            {
                isNegative = true;
                signIndex = 1;
            }
            else if (num[0] == '+')
            {
                signIndex = 1;
            }

            int exp = 1;
            for (int i = num.Length - 1; i >= signIndex; i--)
            {
                if (num[i] < '0' || num[i] > '9')
                {
                    throw new ArgumentException();
                }

                result += ((num[i] - '0') * exp);
                exp *= 10;
            }

            return (isNegative) ? (-1 * result) : result;
        }

        // this method ToInt32 is part of teh Convert class which we will bring over later
        ////////////////////////////////////////////////////////////////////////////////////////


        public static IPAddress GetDefaultLocalAddress()
        {
            // Special conditions are implemented here because of a ptoblem with GetHostEntry
            // on the digi device and NetworkInterface from the emulator.
            // In the emulator we must use GetHostEntry.
            // On the device and Windows NetworkInterface works and is preferred.
            try
            {
                string localAddress = GetDefaultLocalAddressImpl();
                if (string.IsNullOrEmpty(localAddress))
                {
                    return IPAddress.Parse(localAddress);
                }
            }
            catch
            {
            }

            try
            {
                IPAddress localAddress = null;
                IPHostEntry hostEntry = Dns.GetHostEntry("");

                int cnt = hostEntry.AddressList.Length;
                for (int i = 0; i < cnt; ++i)
                {
                    if ((localAddress = hostEntry.AddressList[i]) != null)
                    {
                        if(localAddress.m_Address != 0)
                        {
                            return localAddress;
                        }
                    }
                }
            }
            catch
            {
            }

            return IPAddress.Any;
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private extern static string GetDefaultLocalAddressImpl();

    } // class IPAddress
} // namespace System.Net


