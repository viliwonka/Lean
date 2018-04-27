using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace QuantConnect.Blockchain {

    class Secret {

        byte[] filler0;
        byte[] encryptionKey;
        byte[] filler1;
        byte[] encryptedSecret;
        
        Random random = new Random();

        /*
        public Secret(string rawString) : this(Encoding.UTF8.GetBytes(rawString)) {
                
        }*/

        const int keyPower  = 8;
        const int keyLength = 1 << keyPower;
        const int keyMask   = ~0 << keyPower;

        public Secret(byte[] rawSecret) {
            
            filler0         = new byte[random.Next(13, 29)];
            encryptionKey   = new byte[keyLength];
            filler1         = new byte[random.Next(13, 29)];
            encryptedSecret = new byte[rawSecret.Length];
            
            random.NextBytes(filler0);
            random.NextBytes(encryptionKey);
            random.NextBytes(filler1);

            Array.Copy(rawSecret, encryptedSecret, rawSecret.Length);
            ToggleCrypt();
        }

        /// <summary>
        /// XOR Encrypt/Decrypt
        /// </summary>
        void ToggleCrypt() {

            for(int i = 0; i < encryptedSecret.Length; i++)
                encryptedSecret[i] ^= encryptionKey[i & keyMask];
        }

        public void Read(Action<byte[]> action) {

            // Decrypt
            ToggleCrypt();
            try {
                action(encryptedSecret);
            
            }
            catch(Exception e) {

            // Encrypt
            ToggleCrypt();

                throw e;
            }

        }

    }
}
