using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace TypingGame.Resources
{
	public class Serializer
	{
		public static byte[] SerializeToByteArray(object request)
		{
			byte[] result;
			var serializer = new BinaryFormatter();
			using (var memStream = new MemoryStream())
			{
				serializer.Serialize(memStream, request);
				result = memStream.GetBuffer();
			}
			return result;
		}
		public static SerializeBase DeserializeFromByteArray(byte[] buffer)
		{
			var deserializer = new BinaryFormatter();
			using (var memStream = new MemoryStream(buffer))
			{
				try
				{
					object obj = deserializer.Deserialize(memStream);
					return (SerializeBase)obj;
				}
				catch(Exception e)
				{
					return null;
				}
			}
		}
	}
	[Serializable]
	public abstract class SerializeBase
	{
		public bool IsLessonText;
		public bool IsUserStatictics;
		public bool IsCommand;
	}

	[Serializable]
	public class LessonText: SerializeBase
	{
		public string Lesson;
	}

	[Serializable]
	public class UserStatistics: SerializeBase
	{
		public string CompletionPercentage;
		public string TypedContent;
		public int CharactersTyped;
		public string Accuracy;
		public int Errors;
		public int WPM;
	}

	[Serializable]
	public class Command: SerializeBase
	{
		public bool IsPauseCommand;
		public bool IsGameComplete;
		public bool GameHasStarted;
		public bool StartCommand;
		public bool IsResetCommand;
		public bool ResetIsNewLesson;
		public bool ResetIsRepeatedLesson;
        public string LessonText;
	}

	}
