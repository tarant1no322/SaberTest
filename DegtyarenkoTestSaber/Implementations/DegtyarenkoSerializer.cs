using System;
using System.Text;
using SerializerTests.Interfaces;
using SerializerTests.Nodes;

namespace DegtyarenkoTestSaber.Implementations
{
	public class DegtyarenkoSerializer : IListSerializer
	{
		//the constructor with no parameters is required and no other constructors can be used.
		public DegtyarenkoSerializer()
		{
			//...
		}

		public async Task<ListNode> DeepCopy(ListNode head)
		{
			if (head == null)
			{
				return null;
			}

			var nodeMap = new Dictionary<ListNode, ListNode>();
			var current = head;

			while (current != null)
			{
				nodeMap[current] = new ListNode { Data = current.Data };
				current = current.Next;
			}

			current = head;
			while (current != null)
			{
				nodeMap[current].Previous = current.Previous != null ? nodeMap[current.Previous] : null;
				nodeMap[current].Next = current.Next != null ? nodeMap[current.Next] : null;
				nodeMap[current].Random = current.Random != null ? nodeMap[current.Random] : null;
				current = current.Next;
			}

			return nodeMap[head];
		}

		public async Task<ListNode> Deserialize(Stream s)
		{
			if (s == null || s.Length == 0)
			{
				throw new ArgumentException("Stream is empty or null");
			}

			List<ListNode> nodes = new();
			s.Position = 0;

			using (var br = new BinaryReader(s))
			{
				int listCount = br.ReadInt32();

				for (int i = 0; i < listCount; i++)
				{
					nodes.Add(new ListNode() { Data = br.ReadString() });
				};

				for (int i = 0; i < listCount; i++)
				{
					if (i > 0) nodes[i].Previous = nodes[i - 1];
					if (i < nodes.Count - 1) nodes[i].Next = nodes[i + 1];
				}

				int indexLink;
				int indexRandomNode;

				while (br.BaseStream.Position < br.BaseStream.Length)
				{
					indexLink = br.ReadInt32();
					indexRandomNode = br.ReadInt32();
					nodes[indexLink].Random = nodes[indexRandomNode];
				}
			}
			return nodes[0];
		}

		public async Task Serialize(ListNode head, Stream s)
		{
			var nodes = new List<ListNode>();
			var randomNodeToIndex = new Dictionary<ListNode, int>();

			var current = head;
			var currentIndex = 0;
			s.Position = 0;

			using (var ms = new MemoryStream()){
				using (var bw = new BinaryWriter(ms))
				{
					bw.Write(0); //записывает 0 чтобы зарезервировать 4 байта для записи длины спика в начале потока
					while (current != null)
					{
						if (current.Random != null)
							randomNodeToIndex.Add(current.Random, currentIndex);

						bw.Write(current.Data);

						currentIndex++;
						nodes.Add(current);
						current = current.Next;
					}

					foreach (var key in randomNodeToIndex.Keys)
					{
						int indexRandom = nodes.IndexOf(key);
						if (indexRandom == -1)
							continue;

						bw.Write(randomNodeToIndex[key]);
						bw.Write(indexRandom);
					}

					bw.Seek(0, SeekOrigin.Begin);
					bw.Write(nodes.Count);
					ms.Seek(0, SeekOrigin.Begin);
					ms.CopyTo(s);
				}
			}
			s.Seek(0, SeekOrigin.Begin);

			return;
		}
	}
}
