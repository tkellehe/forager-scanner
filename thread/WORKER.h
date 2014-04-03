#ifndef _WORKER_H_
#define _WORKER_H_

#include <thread>
#include <iostream>
#include <string>
#include <vector>
#include "CEO.h"

using namespace std;

class WORKER
{
public:
	string type;
	CEO my_ceo;
	vector<string> work = vector<string>();
	thread my_thread;
	bool sleeping;

	WORKER();

	WORKER(CEO ceo);

	void GO();

	void IDLE();

	void KILL();

	void WAKE_UP(string type, vector<string> work);

	void DO_WORK();
};
#endif