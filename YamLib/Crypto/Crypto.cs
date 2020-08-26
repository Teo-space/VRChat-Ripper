using System;
using System.IO;
using System.Security.Cryptography;

namespace YamLib.Security
{
	/// <summary>
	///     Implements AES encryption/decryption and ECDH key-exchange
	/// </summary>
	public class Crypto : IDisposable
	{
		private readonly ECDiffieHellmanCng _keyPair;
		private readonly byte[] _publicKey;
		private readonly object l = new object();
		private bool _ready;
		private byte[] _sharedKey;

		/// <summary>
		///     Sets up class, and generates public key
		/// </summary>
		public Crypto()
		{
			_keyPair = new ECDiffieHellmanCng
			{
				KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hash,
				HashAlgorithm = CngAlgorithm.Sha256
			};
			_publicKey = _keyPair.PublicKey.ToByteArray();
		}

		public void Dispose()
		{
			lock (l)
			{
				if (_keyPair != null)
					_keyPair.Dispose();
			}
		}

		/// <summary>
		///     Generates shared private key from another public key
		/// </summary>
		/// <param name="key">
		///     Other public key
		/// </param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="InvalidOperationException"></exception>
		/// <exception cref="ArgumentException"></exception>
		/// <exception cref="CryptographicException"></exception>
		public void EstablishSecretKey(byte[] key)
		{
			lock (l)
			{
				_ready = false;

				try // DEBUG
				{
					_sharedKey = _keyPair.DeriveKeyMaterial(CngKey.Import(key, CngKeyBlobFormat.EccPublicBlob));
				}
				catch (Exception ex) // DEBUG
				{
					System.Console.WriteLine("Exception caught: {0}", ex.Message);
					return;
				}

				_ready = true;
			}
		}

		/// <summary>
		///     Returns public key that was generated at creation of crypto class
		/// </summary>
		/// <returns>
		///     Public key
		/// </returns>
		public byte[] GetPublicKey()
		{
			lock (l)
			{
				return _publicKey;
			}
		}

		/// <summary>
		///     Encrypts data
		/// </summary>
		/// <param name="unencryptedData">
		///     Unencrypted bytearray
		/// </param>
		/// <param name="iv">
		///     Outputs the Initial Vector from the encryption of the data
		/// </param>
		/// <returns>
		///     Encrypted data
		///     <para />
		///     Returns null if input is invalid / ECDH-exchange has not occured
		/// </returns>
		public byte[] Encrypt(byte[] unencryptedData)
		{
			lock (l)
			{
				if (!_ready)
					return null;

				using (Aes aes = new AesCryptoServiceProvider())
				{
					aes.Key = _sharedKey;
					var iv = aes.IV;

					// Encrypt the data
					using (var ms = new MemoryStream())
					{
						using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
						{
							cs.Write(unencryptedData, 0, unencryptedData.Length);
						}

						var encryptedContent = ms.ToArray();

						var result = new byte[iv.Length + encryptedContent.Length];

						//copy our 2 array into one
						Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
						Buffer.BlockCopy(encryptedContent, 0, result, iv.Length, encryptedContent.Length);

						return result;
					}
				}

				return null;
			}
		}

		/// <summary>
		///     Decrypts encrypted data
		/// </summary>
		/// <param name="encryptedData">
		///     Encrypted array of bytes
		/// </param>
		/// <param name="iv">
		///     Initial Vector Output from encryption of message
		/// </param>
		/// <returns>
		///     Returns the unencrypted data, or <c>null</c> if (input is invalid / ECDH-exchange has not occured)
		/// </returns>
		public byte[] Decrypt(byte[] encryptedData)
		{
			lock (l)
			{
				if (!_ready || encryptedData == null || encryptedData.Length <= 16)
					return null;

				var iv = new byte[16];
				var dat = new byte[encryptedData.Length - iv.Length];

				Buffer.BlockCopy(encryptedData, 0, iv, 0, iv.Length);
				Buffer.BlockCopy(encryptedData, iv.Length, dat, 0, dat.Length);

				using (Aes aes = new AesCryptoServiceProvider())
				{
					aes.Key = _sharedKey;
					aes.IV = iv;

					// Decrypt the data
					using (var ms = new MemoryStream())
					{
						using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
						{
							cs.Write(dat, 0, dat.Length);
						}

						return ms.ToArray();
					}
				}
			}
		}

		/// <summary>
		///     Generates a unique token of specified length using cryptographic functions
		/// </summary>
		/// <param name="length">
		///     Length of output token
		/// </param>
		/// <returns>
		///     The generated token
		/// </returns>
		/// <exception cref="CryptographicException"></exception>
		public static string GetUniqueToken(int length)
		{
			const string charList =
				"ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/-_=?!#&%()@${[]}*.:,;<>";

			using (var crypto = new RNGCryptoServiceProvider())
			{
				var data = new byte[length];

				// If chars.Length isn't a power of 2 then there is a bias if we simply use the modulus operator. The first characters of chars will be more probable than the last ones.
				// buffer used if we encounter an unusable random byte. We will regenerate it in this buffer
				byte[] buffer = null;

				// Maximum random number that can be used without introducing a bias
				var maxRandom = byte.MaxValue - (byte.MaxValue + 1) % charList.Length;

				crypto.GetBytes(data);

				var result = new char[length];

				for (var i = 0; i < length; i++)
				{
					var value = data[i];

					while (value > maxRandom)
					{
						if (buffer == null) buffer = new byte[1];

						crypto.GetBytes(buffer);
						value = buffer[0];
					}

					result[i] = charList[value % charList.Length];
				}

				return new string(result);
			}
		}
	}
}