#ifndef _WORKER_H_
#define _WORKER_H_

#include <thread>
#include <iostream>
#include <string>
#include <vector>
#include "LOGGER.h"

using namespace std;

class WORKER
{
public:
	string type,prev_type;
	LOGGER log;
	vector<string> work;
	bool sleeping, done = false, checked = false;

	WORKER();
	WORKER(LOGGER log);

	void WAKE_UP(string type, vector<string> work);
};

WORKER::WORKER(){ type = "idle"; prev_type = "idle"; }

WORKER::WORKER(LOGGER log)
{
	this->log = log;
	type = "idle";
	prev_type = "idle";
	
}

void WORKER::WAKE_UP(string type, vector<string> work)
{
	this->type = type;
	this->work = work;
	//wake him up
	sleeping = false;
	checked = false;
}

#endif