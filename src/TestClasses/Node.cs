namespace StandardDot.TestClasses
{
	public class Node
	{
		private Node _prev;
		private Node _next;
		private string name;
		public Node Prev { get { return _prev; } set { _prev = value; } }
		public Node Next { get { return _next; } set { _next = value; } }
		public string Name { get { return name; } set { name = value; } }
	}
}