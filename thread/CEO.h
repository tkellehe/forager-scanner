#ifndef _CEO_H_
#define _CEO_H_

#include <thread>
#include <iostream>
#include <string>
#include <vector>
#include "WORKER.h"
#include "LOGGER.h"

using namespace std;

class CEO
{
public:
	WORKER *workers;
	thread *threads;
	vector<vector<string>>
		scan_work,
		move_work,
		test_work,
		update_work;
	unsigned
		scan_working_count = 0,
		move_working_count = 0,
		test_working_count = 0,
		update_working_count = 0,
		total_thread_count,
		max_work = 0,//if zero do unlimited//refers to the amount to scan
		work_counter = 0;
	bool stop = false, done = false;
	CEO();
	CEO(int amount_of_threads, vector<string> start_work);

	CEO(int amount_of_threads, vector<string> start_work, int max_work);
	
	void BASIC_INIT(int amount_of_threads, vector<string> start_work);

	unsigned IDLE_COUNT();
};

CEO::CEO(){}

CEO::CEO(int amount_of_threads, vector<string> start_work)
{
	BASIC_INIT(amount_of_threads, start_work);
}

CEO::CEO(int amount_of_threads, vector<string> start_work, int max_work)
{
	this->max_work = max_work;
	BASIC_INIT(amount_of_threads, start_work);
}

unsigned CEO::IDLE_COUNT()
{
	return total_thread_count - scan_working_count - move_working_count - test_working_count - update_working_count;
}

void CEO::BASIC_INIT(int amount_of_threads, vector<string> start_work)
{
	//create all of the workers needed
	workers = new WORKER[amount_of_threads];
	//creats all of the threads to ru the workers
	threads = new thread[amount_of_threads];
	//grab total workers can have
	total_thread_count = (unsigned)amount_of_threads;
	//actually initialize the threads
	for (int i = 0; i < amount_of_threads; ++i)
		workers[i] = WORKER(LOGGER());
	//give start_work to move_work to get something started
	for (unsigned i = 0; i < start_work.size(); ++i)
	if (start_work[i][start_work[i].size() - 1] == '/')
		move_work.push_back(vector<string>{start_work[i]});
	else
		move_work.push_back(vector<string>{start_work[i] + '/'});
}

#endif