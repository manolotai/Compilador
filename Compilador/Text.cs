using System.IO;
using Jace;

NameSpace Test.subPath{ //Prueba
	public Class CTest{
		public void Main(){
			float t1 = (1 + 1) + 2 * 4;
			
			for(/***/int i = 0; i < 5; i++){
				t1 = i;
				Console.Write(i);				
			}

			if(false){
				
			} else if ( !false ){
				int t2 = (int) t1 % 3;
				Console.WriteLine(t2);
				Console.ReadLine();
			} else 
				Console.WriteLine(t1);
			
			Console.ReadKey();
		}
	}
}