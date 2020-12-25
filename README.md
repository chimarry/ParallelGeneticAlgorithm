# UWP application "Genetic algorithm"
## Implemented:
- Logic for a genetic algorithm that, based on specified operands, finds a specified number and generates expression. (*Brackets in most cases are not matched, but expression should be understood*)
- Job consists of a list of numbers that need to be generated using a genetic algorithm.
- Job can be specified through file with .ga extension (double click on that file or load file through GUI - in which case folder must be chosen), or can be created through GUI form.
- Jobs are run in parallel with *level of parallelism =  Job.MaxLevelOfParallelism*. Each job is parallelised (the number of threads is specified in job's configuration), but there is a limit for parallelisation (*Job.MaxLevelOfParallelismPerJob*)
- Jobs can be started, stopped, resumed, cancelled.
- Each job has a status: Pending, Started, Cancelled, Resumed and Finished. Also, each unit of a job (the number that needs to be found) has an undetermined progress ring.
- Before jobs are started, the folder must be chosen to save results. The folder is chosen only once during runtime. When job finishes (all job units are completed), expressions are converted to images, and saved to specified results' folder.
- In case of error, a content dialog is shown.
## Not implemented:
- Background tasks
- Job persistency after the application closes