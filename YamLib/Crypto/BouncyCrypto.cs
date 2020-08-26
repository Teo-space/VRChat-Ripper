/*
using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Security;
using System;
using System.IO;
using System.Text;

namespace YamLib.Security
{
	/// <summary>
	/// Implements AES encryption/decryption and ECDH key-exchange
	/// </summary>
	public class BouncyCrypto
	{
		private bool _ready = false;
		private X9ECParameters m_x9EC;

		private ECPublicKeyParameters m_myPubKey;
		private AsymmetricKeyParameter m_myPrivKey;

		private byte[] m_secretKey = null;

		/// <summary>
		/// Sets up class, and generates public key
		/// </summary>
		public BouncyCrypto()
		{
			m_x9EC = NistNamedCurves.GetByName("P-521");
			ECDomainParameters ecDomain = new ECDomainParameters(m_x9EC.Curve, m_x9EC.G, m_x9EC.N, m_x9EC.H, m_x9EC.GetSeed());

			ECKeyPairGenerator g = (ECKeyPairGenerator)GeneratorUtilities.GetKeyPairGenerator("ECDH");
			g.Init(new ECKeyGenerationParameters(ecDomain, new SecureRandom()));

			AsymmetricCipherKeyPair keyPair = g.GenerateKeyPair();

			m_myPubKey = (ECPublicKeyParameters)keyPair.Public;
			m_myPrivKey = keyPair.Private;
		}

		/// <summary>
		/// Generates shared private key from another public key
		/// </summary>
		/// <param name="key">
		/// Other public key
		/// </param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="InvalidOperationException"></exception>
		/// <exception cref="ArgumentException"></exception>
		/// <exception cref="CryptographicException"></exception>
		public void EstablishSecretKey(byte[] key)
		{
			_ready = false;
			if (key == null)
			{
				throw new ArgumentNullException();
			}

			try // DEBUG
			{
				string str = Encoding.UTF8.GetString(key);
				string[] elements = str.Split(',');
				if (elements.Length != 2)
					throw new ArgumentException();

				ECPoint point = m_x9EC.Curve.CreatePoint(
					new BigInteger(Convert.FromBase64String(elements[0])),
					new BigInteger(Convert.FromBase64String(elements[1]))
					);

				ECPublicKeyParameters remotePubKey = new ECPublicKeyParameters("ECDH", point, SecObjectIdentifiers.SecP521r1);

				IBasicAgreement aKeyAgree = AgreementUtilities.GetBasicAgreement("ECDH");
				aKeyAgree.Init(m_myPrivKey);

				byte[] secretKey = aKeyAgree.CalculateAgreement(remotePubKey).ToByteArray();
				m_secretKey = new byte[16];
				System.Buffer.BlockCopy(secretKey, 0, m_secretKey, 0, 16);

			}
			catch (Exception ex) // DEBUG
			{
				Console.WriteLine("Exception caught: {0}", ex.Message);
				return;
			}

			_ready = true;
		}

		/// <summary>
		/// Returns public key that was generated at creation of crypto class
		/// </summary>
		/// <returns>
		/// Public key
		/// </returns>
		public byte[] GetPublicKey()
		{
			string str = string.Format(
				"{0},{1}",
				Convert.ToBase64String(m_myPubKey.Q.AffineXCoord.ToBigInteger().ToByteArray()),
				Convert.ToBase64String(m_myPubKey.Q.AffineYCoord.ToBigInteger().ToByteArray())
				);
			return Encoding.UTF8.GetBytes(str);
		}

		/// <summary>
		/// Encrypts data
		/// </summary>
		/// <param name="unencryptedData">
		/// Unencrypted bytearray
		/// </param>
		/// <returns>
		/// Encrypted data<para/>
		/// Returns null if input is invalid / ECDH-exchange has not occured
		/// </returns>
		public byte[] Encrypt(byte[] unencryptedData)
		{
			if (!_ready || unencryptedData == null)
				return null;

			try // DEBUG
			{
				using (MemoryStream ms = new MemoryStream())
				{
					using (System.Security.Cryptography.AesManaged cryptor = new System.Security.Cryptography.AesManaged())
					{
						cryptor.Mode = System.Security.Cryptography.CipherMode.CBC;
						cryptor.Padding = System.Security.Cryptography.PaddingMode.PKCS7;
						cryptor.KeySize = 128;
						cryptor.BlockSize = 128;

						byte[] iv = cryptor.IV;

						using (System.Security.Cryptography.CryptoStream cs = new System.Security.Cryptography.CryptoStream(ms, cryptor.CreateEncryptor(m_secretKey, iv), System.Security.Cryptography.CryptoStreamMode.Write))
							cs.Write(unencryptedData, 0, unencryptedData.Length);

						byte[] encryptedContent = ms.ToArray();

						byte[] result = new byte[iv.Length + encryptedContent.Length];

						//copy our 2 array into one
						System.Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
						System.Buffer.BlockCopy(encryptedContent, 0, result, iv.Length, encryptedContent.Length);

						return result;
					}
				}
			}
			catch (Exception ex) // DEBUG
			{
				Console.WriteLine("Exception caught: {0}", ex.Message);
			}
			return null;
		}

		/// <summary>
		/// Decrypts encrypted data
		/// </summary>
		/// <param name="encryptedData">
		/// Encrypted array of bytes
		/// </param>
		/// <returns>
		/// Returns the unencrypted data, or <c>null</c> if (input is invalid / ECDH-exchange has not occured)
		public byte[] Decrypt(byte[] encryptedData)
		{
			if (!_ready || encryptedData == null)
				return null;

			byte[] iv = new byte[16];
			byte[] dat = new byte[encryptedData.Length - iv.Length];

			System.Buffer.BlockCopy(encryptedData, 0, iv, 0, iv.Length);
			System.Buffer.BlockCopy(encryptedData, iv.Length, dat, 0, dat.Length);

			try // DEBUG
			{
				using (MemoryStream ms = new MemoryStream())
				{
					using (System.Security.Cryptography.AesManaged cryptor = new System.Security.Cryptography.AesManaged())
					{
						cryptor.Mode = System.Security.Cryptography.CipherMode.CBC;
						cryptor.Padding = System.Security.Cryptography.PaddingMode.PKCS7;
						cryptor.KeySize = 128;
						cryptor.BlockSize = 128;

						// Decrypt the data
						using (System.Security.Cryptography.CryptoStream cs = new System.Security.Cryptography.CryptoStream(ms, cryptor.CreateDecryptor(m_secretKey, iv), System.Security.Cryptography.CryptoStreamMode.Write))
							cs.Write(dat, 0, dat.Length);

						return ms.ToArray();
					}
				}
			}
			catch (Exception ex) // DEBUG
			{
				Console.WriteLine("Exception caught: {0}", ex.Message);
			}
			return null;
		}
	}
}
*/

