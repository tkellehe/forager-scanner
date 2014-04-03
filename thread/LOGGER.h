#ifndef _LOGGER_H_
#define _LOGGER_H_

#include <string>
#include <vector>

using namespace std;

class LOGGER
{
public:
	vector<vector<string>> my_info;
	LOGGER();

	void LOG(string type, vector<string>info);

	vector<vector<string>> DUMP();
};

LOGGER::LOGGER(){ my_info = vector<vector<string>>(); }
void LOGGER::LOG(string type, vector<string>info)
{
	info.push_back(type);
	my_info.push_back(info);
}

vector<vector<string>> LOGGER::DUMP()
{
	vector<vector<string>> r;
	for (unsigned i = 0; i < my_info.size(); ++i)
	{
		my_info[i].pop_back();
		r.push_back(my_info[i]);
	}
	my_info.clear();
	return r;
}
#endif