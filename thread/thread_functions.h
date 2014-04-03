#ifndef _THREADING_H_
#define _THREADING_H_

#include <thread>
#include <iostream>
#include <string>
#include <vector>
#include "WORKER.h"
#include "CEO.h"
#include "LOGGER.h"

class threading
{
public:
	void CGO(CEO *ceo);
	void ASSIGN_WORK(CEO *ceo);
	string CONFIG_BEST(CEO *ceo);

	void WGO(WORKER *worker);
	void IDLE(WORKER *worker);
	void DO_WORK(WORKER *worker);
};

#pragma region WORKER thread functions
void threading::WGO(WORKER *worker)
{
	DO_WORK(worker);
}

void threading::IDLE(WORKER *worker)
{
	worker->sleeping = true;
	while (worker->sleeping && !worker->done)
		this_thread::sleep_for(std::chrono::milliseconds(10));
}
void threading::DO_WORK(WORKER *worker)
{
	cout << "starting to do work: "<< this_thread::get_id()<< endl;
	while (!worker->done)
	if (worker->type == "idle")
	{
		//sleep thread
		IDLE(worker);
	}
	else if (worker->type == "scan")
	{
		//call scan methods
		if (worker->work[0] == "1/")
		{
			for (int i = 0; i < 4; i++)
				worker->log.LOG(worker->type, vector<string>{ "2", "1/" });//ID-l-s, FID-l-s
		}
		//clear work
		worker->work.clear();
		//record prev type
		worker->prev_type = worker->type;
		//make idle
		worker->type = "idle";
	}
	else if (worker->type == "move")
	{
		//call move methods
		worker->log.LOG(worker->type, vector<string>{worker->work[0]});
		//clear work
		worker->work.clear();
		//record prev type
		worker->prev_type = worker->type;
		//make idle
		worker->type = "idle";
	}
	else if (worker->type == "test")
	{
		//call test methods
		string w_type = "bad";
		if (worker->work[0] == "1/")
			w_type = "good";
		worker->log.LOG(worker->type, vector<string>{ worker->work[0], w_type });
		//clear work
		worker->work.clear();
		//record prev type
		worker->prev_type = worker->type;
		//make idle
		worker->type = "idle";
	}
	else if (worker->type == "update")
	{
		//move good stuff to scan
		if (worker->work[1] == "good")
			worker->log.LOG(worker->type, vector<string>{ worker->work[0] });
		//call update methods
		//clear work
		worker->work.clear();
		//record prev type
		worker->prev_type = worker->type;
		//make idle
		worker->type = "idle";
	}
}
#pragma endregion

#pragma region CEO thread functions
void threading::CGO(CEO *ceo)
{
	for (unsigned i = 0; i < ceo->total_thread_count; ++i)
		ceo->threads[i] = thread(&threading::WGO, new threading(), &ceo->workers[i]);
	ASSIGN_WORK(ceo);
}

void threading::ASSIGN_WORK(CEO *ceo)
{
	cout << "starting to assign work: " << this_thread::get_id() << endl;
	while (!ceo->done)
	{
		for (unsigned i = 0; i < ceo->total_thread_count; ++i)
		{
			//Grab if the worker is idle
			if (ceo->workers[i].type == "idle")
			{
				//grab the log of the current worker
				string prev = ceo->workers[i].prev_type;
				vector<vector<string>> log = ceo->workers[i].log.DUMP();

				if (prev != "idle" && !ceo->workers[i].checked)
				{
					//decrement counter for previous work
					unsigned *count =
						prev == "scan" ? &ceo->scan_working_count :
						prev == "move" ? &ceo->move_working_count :
						prev == "test" ? &ceo->test_working_count :
						&ceo->update_working_count;
					(*count)--;
					ceo->workers[i].checked = true;
				}

				//if there is anything to log then log it.
				if (log.size() > 0)
				{
					//find the correct vector to place the work in.
					vector<vector<string>> *dump_to =
						prev == "scan" ? &ceo->move_work :
						prev == "move" ? &ceo->test_work :
						prev == "test" ? &ceo->update_work :
						&ceo->scan_work;
					//push the logs into the appropriate work
					for (unsigned i = 0; i < log.size(); ++i)
						dump_to->push_back(log[i]);
				}
				//Configure which work needs to be done right away
				string needs_work = CONFIG_BEST(ceo);
				//make sure that there is something to work on
				if (needs_work != "nothing")
				{
					//wake up the worker and get him working
					ceo->workers[i].WAKE_UP(needs_work,
						needs_work == "scan" ? ceo->scan_work[ceo->scan_work.size() - 1] :
						needs_work == "move" ? ceo->move_work[ceo->move_work.size() - 1] :
						needs_work == "test" ? ceo->test_work[ceo->test_work.size() - 1] :
						ceo->update_work[ceo->update_work.size() - 1]);
					//pop off of the back
					vector<vector<string>> *pop_from =
						needs_work == "scan" ? &ceo->scan_work :
						needs_work == "move" ? &ceo->move_work :
						needs_work == "test" ? &ceo->test_work :
						&ceo->update_work;
					pop_from->pop_back();
					//increment counter for previous work
					unsigned *count =
						needs_work == "scan" ? &ceo->scan_working_count :
						needs_work == "move" ? &ceo->move_working_count :
						needs_work == "test" ? &ceo->test_working_count :
						&ceo->update_working_count;
					(*count)++;
				}
			}
			cout << "IDLE COUNT: "<< ceo->IDLE_COUNT() << endl;
		}
		//if everything is zero then we are completely done
		ceo->done = ceo->scan_working_count == 0 && ceo->scan_work.size() == 0 &&
			ceo->move_working_count == 0 && ceo->move_work.size() == 0 &&
			ceo->test_working_count == 0 && ceo->test_work.size() == 0 &&
			ceo->update_working_count == 0 && ceo->update_work.size() == 0;
	}
	//clean up time...
	for (unsigned i = 0; i < ceo->total_thread_count; ++i)
		ceo->workers[i].done = true;
	delete[] ceo->workers;
	//assign to database that we are done
	cout << "DONE!!: " << endl;
}

string threading::CONFIG_BEST(CEO *ceo)
{
	//find the ratio of the amount of work to be done and the amount of work being done...
	float a = (float)ceo->scan_work.size() / (ceo->scan_working_count == 0 ? 1 : ceo->scan_working_count);
	float b = (float)ceo->move_work.size() / (ceo->move_working_count == 0 ? 1 : ceo->move_working_count);
	float c = (float)ceo->test_work.size() / (ceo->test_working_count == 0 ? 1 : ceo->test_working_count);
	float d = (float)ceo->update_work.size() / (ceo->update_working_count == 0 ? 1 : ceo->update_working_count);
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
#pragma endregion
#endif