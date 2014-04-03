#include "CEO.h"

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

void CEO::BASIC_INIT(int amount_of_threads, vector<string> start_work)
{
	//create all of the workers needed
	workers = new WORKER[amount_of_threads];
	//start everyone as idle
	idle_count = (unsigned)amount_of_threads;
	//grab total workers can have
	total_thread_count = (unsigned)amount_of_threads;
	//actually initialize the threads
	for (int i = 0; i < amount_of_threads; ++i)
		workers[i] = WORKER(*this);
	//the thread in charge of organizing work
	my_thread = thread(&CEO::ASSIGN_WORK, this);
	//give start_work to move_work to get something started
	for (unsigned i = 0; i < start_work.size(); ++i)
	if (start_work[i][start_work[i].size() - 1] == '/')
		move_work.push_back(vector<string>{start_work[i]});
	else
		move_work.push_back(vector<string>{start_work[i] + '/'});
}
void CEO::GO()
{
	for (unsigned i = 0; i < total_thread_count; ++i)
		workers[i].GO();
	my_thread.join();
}
void CEO::ASSIGN_WORK()
{
	while (!done)
	{
		for (unsigned i = 0; i < total_thread_count; ++i)
		{
			if (idle_count == 0)
				break;
			if (workers[i].type == "idle")
			{
				string needs_work = CONFIG_BEST();
				if (needs_work == "nothing")
					break;
				workers[i].WAKE_UP(needs_work,
					needs_work == "scan" ? scan_work[0]
					: needs_work == "move" ? move_work[0]
					: needs_work == "test" ? test_work[0]
					: update_work[0]);
			}
		}
		//if everything is zero then we are completely done
		done = scan_working_count == 0 && scan_work.size() == 0 &&
			move_working_count == 0 && move_work.size() == 0 &&
			test_working_count == 0 && test_work.size() == 0 &&
			update_working_count == 0 && update_work.size() == 0;
	}
	//clean up time...
	for (unsigned i = 0; i < total_thread_count; ++i)
		workers[i].KILL();
	delete[] workers;
	//assign to database that we are done
}
string CEO::CONFIG_BEST()
{
	//find the ratio of the amount of work to be done and the amount of work being done...
	float a = (float)scan_work.size() / (scan_working_count == 0 ? 1 : scan_working_count);
	float b = (float)move_work.size() / (move_working_count == 0 ? 1 : move_working_count);
	float c = (float)test_work.size() / (test_working_count == 0 ? 1 : test_working_count);
	float d = (float)update_work.size() / (update_working_count == 0 ? 1 : update_working_count);
	//if everything is empty then there is nothing to do.
	if (a == 0 && b == 0 && c == 0 && d == 0)
		return "nothing";
	string w = "scan";
	if (b > a)
	{
		w = "move";
		a = b;
	}
	if (c > a)
	{
		w = "test";
		a = c;
	}
	if (d > a)
	{
		w = "update";
		a = d;
	}
	return w;
}
