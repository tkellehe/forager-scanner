#include <thread>
#include <iostream>
#include <string>
#include <vector>
#include "CEO.h"
#include "WORKER.h"
#include "thread_functions.h"


using namespace std;
//class BAR
//{
//public:
//	bool work = true;
//	BAR();
//};
//
//BAR::BAR()
//{
//}
//
//class threading
//{
//public:
//	thread my_thread;
//	bool done = false;
//	threading();
//	void stuff();
//	void GO(BAR *b);
//	
//};
//
//threading::threading()
//{
//
//}
//
//void threading::stuff()
//{
//	//while (!done)
//	cout << "lol: "<<this_thread::get_id<<endl;
//}
//
//void threading::GO(BAR *b)
//{
//	//my_thread = thread(&threading::stuff, this);
//	while (b->work)
//	{
//		stuff();
//		this_thread::sleep_for(std::chrono::seconds(1));
//	}
//}

void main()
{
	CEO *ceo = new CEO(4, vector<string>{"1"}, 3);

	thread runner = thread(&threading::CGO, new threading(), ceo);
	//BAR *b = new BAR();
	//thread t = thread(&threading::GO, new threading(), b);
	//this_thread::sleep_for(std::chrono::seconds(5));
	//	b->work = false;
	system("pause");
}