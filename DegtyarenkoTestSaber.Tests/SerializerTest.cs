using System.Linq;
using DegtyarenkoTestSaber.Implementations;
using SerializerTests.Implementations;
using SerializerTests.Nodes;

namespace DegtyarenkoTestSaber.Tests
{
    public class SerializerTest
	{
		[Test]
		public void DeepCloneTest()
        {
			var head = GenerateDoublyLinkedList();

            //
			List<string?> s2 = new List<string?>();
			List<string?> s1 = new List<string?>();
			var cur = head;
			while (cur != null)
			{
				s2.Add(cur.Data);
				s1.Add(cur.Random?.Data ?? null);
				cur = cur.Next;
			}
            //

			var serializer = new ListSerializer();
            var result = serializer.DeepCopy(head);
            Assert.IsFalse(object.Equals(result, head));
		}

		[Test]
		public void Serialize_Deserialize_Test()
		{
            var head = GenerateDoublyLinkedList();

            //
			List<string?> s2 = new List<string?>();
			List<string?> s1 = new List<string?>();
            var cur = head;
            while(cur != null)
            {
                s2.Add(cur.Data);
                s1.Add(cur.Random?.Data ?? null);
                cur = cur.Next;
            }
            //

			var serializer = new ListSerializer();
            Stream s = new MemoryStream();
            serializer.Serialize(head, s);

            ListNode ln = serializer.Deserialize(s).Result;
            Assert.True(true);
        }

        private string GenerateRandomString(Random random)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            int length = random.Next(1, 21);
            char[] result = new char[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = chars[random.Next(chars.Length)];
            }
            return new string(result);
        }

        private ListNode GenerateDoublyLinkedList()
        {
            int count = 10;

            List<ListNode> nodes = new List<ListNode>(count);
            Random random = new Random();
            for(int i = 0; i < count; i++)
            {
				nodes.Add(new ListNode() { Data = GenerateRandomString(random) });
			}
			for (int i = 0; i < count; i++)
			{
				if (i > 0) nodes[i].Previous = nodes[i - 1];
                if (i < count - 1) nodes[i].Next = nodes[i + 1];
            }

            ListNode head = nodes[0];
            ListNode current = head;

            var freeRandomIndex = new Stack<int>(new int[] { 6, 2, -1, 4, -1, 0, 3, 8, 9, 7});
            int tempIndex;

            while (current.Next != null)
            {
				tempIndex = freeRandomIndex.Pop();
                if (tempIndex != -1)
                    current.Random = nodes[tempIndex];
                current = current.Next;
            }

            return head;
        }
    }
}