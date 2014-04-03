#include "WORKER.h"

WORKER::WORKER(){ type = "idle"; }

WORKER::WORKER(CEO ceo)
{
	my_ceo = ceo;
	type = "idle";
	my_thread = thread(&WORKER::DO_WORK, this);
}

void WORKER::GO()
{
	my_thread.join();
}

void WORKER::IDLE()
{
	sleeping = true;
	while (sleeping)
		this_thread::sleep_for(std::chrono::milliseconds(10));
}

void WORKER::KILL()
{
	std::terminate();
}

void WORKER::WAKE_UP(string type, vector<string> work)
{
	this->type = type;
	this->work = work;
	//wake him up
	sleeping = false;
}

void WORKER::DO_WORK()
{
	while (true)
	if (type == "idle")
	{
		//sleep thread
		IDLE();
	}
	else if (type == "scan")
	{
		//call scan methods
		if (work[0] == "1/")
		{
			for (int i = 0; i < 4; i++)
				my_ceo.move_work.push_back(vector<string>{"1/", "1/2"});
		}
		//clear work
		work.clear();
		//make idle
		type = "idle";
		//decrease scan_worker_count in CEO
		my_ceo.scan_working_count--;
		//increase idel_count in CEO
		my_ceo.idle_count++;
	}
	else if (type == "move")
	{
		//call move methods
		my_ceo.test_work.push_back(vector<string>{work[0]});
		//clear work
		work.clear();
		//make idle
		type = "idle";
		//decrease move_worker_count in CEO
		my_ceo.move_working_count--;
		//increase idel_count in CEO
		my_ceo.idle_count++;
	}
	else if (type == "test")
	{
		//call test methods
		string w_type = "bad";
		if (work[0] == "1/")
			w_type = "good";
		my_ceo.update_work.push_back(vector<string>{work[0], w_type});
		//clear work
		work.clear();
		//make idle
		type = "idle";
		//decrease test_worker_count in CEO
		my_ceo.test_working_count--;
		//increase idel_count in CEO
		my_ceo.idle_count++;
	}
	else if (type == "update")
	{
		//move good stuff to scan
		if (my_ceo.max_work == 0 ? true : my_ceo.max_work > my_ceo.work_counter && work[1] == "good")
		{
			my_ceo.work_counter++;
			my_ceo.scan_work.push_back(vector<string>{work[0]});
		}
		//call update methods
		//clear work
		work.clear();
		//make idle
		type = "idle";
		//decrease update_worker_count in CEO
		my_ceo.update_working_count--;
		//increase idel_count in CEO
		my_ceo.idle_count++;
	}
}