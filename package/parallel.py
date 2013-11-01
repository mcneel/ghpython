import System.Threading.Tasks as tasks
from System import Exception, AggregateException

def run(function, data_list):
    """for each item in data_list execute the input function. Execution is
    done on as many threads as there are CPUs on the computer.
    Parameters:
        function: function to execute for each item in the data_list
        data_list: list, tuple, or other enumerable data structure
    Returns:
        list of results containing the return from each call to the input function
    """
    pieces = [(i,data) for i,data in enumerate(data_list)]
    results = range(len(pieces))

    def helper(piece):
        i, data = piece
        local_result = function(data)
        results[i] = local_result
    # Run first piece serial in case there is "set up" code in the function
    # that needs to be done once. All other iterations are done parallel
    helper(pieces[0])
    pieces = pieces[1:]
    if pieces: tasks.Parallel.ForEach(pieces, helper)
    return results


def chunk_list(l,n):
    """Break a list into a list of lists"""
    if n<2: return l
    return [l[i:i+n] for i in range(0, len(l), n)]
