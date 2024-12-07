using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SerializerTests.Interfaces;
using SerializerTests.Nodes;

namespace SerializerTests.Implementations
{
    public class ListSerializer : IListSerializer
    {
        public async Task Serialize(ListNode head, Stream s)
        {
            if (head == null)
            {
                throw new ArgumentNullException(nameof(head));
            }

            var nodeMap = new Dictionary<ListNode, long>();
            var writer = new BinaryWriter(s);
            var current = head;
            long position = 0;

            // First pass: serialize the nodes and store positions
            while (current != null)
            {
                writer.Write(current.Data ?? string.Empty);
                nodeMap[current] = position;
                position = s.Position;

                // Write the position of the Previous, Next, and Random nodes.
                writer.Write((current.Previous != null) ? nodeMap[current.Previous] : -1);
                writer.Write((current.Next != null) ? nodeMap[current.Next] : -1);
                writer.Write((current.Random != null) ? nodeMap[current.Random] : -1);

                current = current.Next;
            }

            // Reset the stream position to the beginning
            s.Position = 0;
            var tempList = new List<ListNode>(nodeMap.Keys);

            // Second pass: write the positions of all nodes (random links)
            writer = new BinaryWriter(s);
            foreach (var node in tempList)
            {
                writer.Write(nodeMap[node]);
            }

            await Task.CompletedTask;
        }

        public async Task<ListNode> Deserialize(Stream s)
        {
            if (s == null || s.Length == 0)
            {
                throw new ArgumentException("Stream is empty or null.");
            }

            var reader = new BinaryReader(s);
            var nodeMap = new Dictionary<long, ListNode>();
            ListNode head = null, lastNode = null;
            long position;

            // First pass: read nodes and create new ListNode instances
            while (s.Position < s.Length)
            {
                string data = reader.ReadString();
                long prevPos = reader.ReadInt64();
                long nextPos = reader.ReadInt64();
                long randomPos = reader.ReadInt64();

                ListNode node = new ListNode { Data = data };
                nodeMap[s.Position] = node; // Store the node by its position for linking later

                if (head == null)
                {
                    head = node;
                }
                else
                {
                    lastNode.Next = node;
                    node.Previous = lastNode;
                }
                lastNode = node;
            }

            // Second pass: set up the random links
            foreach (var node in nodeMap.Values)
            {
                long randomPos = reader.ReadInt64();
                node.Random = randomPos != -1 ? nodeMap[randomPos] : null;
            }

            return head;
        }

        public async Task<ListNode> DeepCopy(ListNode head)
        {
            if (head == null)
            {
                return null;
            }

            var nodeMap = new Dictionary<ListNode, ListNode>();
            var current = head;

            // First pass: create a shallow copy of the nodes
            while (current != null)
            {
                nodeMap[current] = new ListNode { Data = current.Data };
                current = current.Next;
            }

            // Second pass: set up the links between nodes
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
    }
}