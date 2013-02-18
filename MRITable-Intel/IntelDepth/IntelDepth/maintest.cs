using System;

public class maintest
{
	public maintest()
	{
        Publisher pub = new Publisher();
        Subscriber sub1 = new Subscriber("sub1", pub);
        Subscriber sub2 = new Subscriber("sub2", pub);

        // Call the method that raises the event.
        pub.DoSomething();

        // Keep the console window open
        Console.WriteLine("Press Enter to close this window.");
        Console.ReadLine();
	}
}
