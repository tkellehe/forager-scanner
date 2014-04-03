#ifndef _CEO_H_
#define _CEO_H_

#include <thread>
#include <iostream>
#include <string>
#include <vector>
#include "WORKER.h"

using namespace std;

class CEO
{
public:
	WORKER *workers;
	vector<vector<string>>
		scan_work = vector<vector<string>>(),
		move_work = vector<vector<string>>(),
		test_work = vector<vector<string>>(),
		update_work = vector<vector<string>>();
	unsigned
		scan_working_count = 0,
		move_working_count = 0,
		test_working_count = 0,
		update_working_count = 0,
		idle_count,
		total_thread_count,
		max_work = 0,//if zero do unlimited//refers to the amount to scan
		work_counter = 0;
	thread my_thread;
	bool stop = false, done = false;
	CEO();
	CEO(int amount_of_threads, vector<string> start_work);

	CEO(int amount_of_threads, vector<string> start_work, int max_work);


	void BASIC_INIT(int amount_of_threads, vector<string> start_work);
	void GO();
	void ASSIGN_WORK();
	string CONFIG_BEST();
};

#endif