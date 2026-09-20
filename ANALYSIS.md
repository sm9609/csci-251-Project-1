Samuel Mensah's Analysis
### Bug 1: BuggyBank
### Race Condition Location
Lines 62-65 in method `Transfer()`
### Shared State Involved
- `_balance` field accessed by multiple threads
### Why It's a Bug
    When a shared variable like `_balance` is unsynced and touched by multiple threads at the same time it leads to unpredictable outcomes. 
    The Transfer() method uses Withdraw() & Deposit() across multiple threads meaning that it is possable for a thread to read the shared balance value inbetween the process of another thread updating that value. When the thread that was inbetween operation finishes its operation will be lost becuaese the secound thread read the value before the operation happened and is basing its update off of a stale variable. 
### Your Fix
    The approch I took to fix this race condition was syncronizing both Withdraw() and Deposit(). After identifying that they both Read-Modify-Write the Balance I concluded that lock the code right before they need balance was the best choice. This way only on thread has the ability to RMW deposit at a time.  
### Why Your Fix Works
    The Fix works because Tranfer() is able to run on multiple threads at once. but only one thread is allowed to withdraw or deposit at a time.

### Bug 2: BuggyCounter
### Race Condition Location
Lines 22 in method `Increment()`
Lines 28 in method `IncrementBy(int amount)`
### Shared State Involved
- `_count` field accessed by multiple threads
### Why It's a Bug
    Though count++ may look atomic due to its overall simplicity it reads count modifies it by 1 or the amount specified then writes it to shared memory. Becuase this code can RMW a shared variable unsynced threads can overwrite each other and in the process delete some of the increment operations cuaseing a miss count.  
### Your Fix
    The critical section is only one operation and the shared value is an integer, Their for interlocking the operation is enough to syncronize the threads.
### Why Your Fix Works
    The increment operations are not atomic however, interlocked is an easy way to make numerical operations Atomic.


### Bug 3: BuggyCache 
### Race Condition Location
Lines 33-39 in method `GetOrCompute(string key)`
### Shared State Involved
- `_cache` field accessed by multiple threads
### Why It's a Bug
    The code block uses multiple threads to get a value from the cache and during this process multiple threads are checking the same value at the same time. For exapmle 3 threads check for the key its not their now those 3 threads are wasted on an expensive calculation. 
### Your Fix
    An easy fix for an error like this sycronizing the GetOrCompute() method with a single lock. I choose a lock because we only want one thread at a time and the critical section has to check and or update shared memory.
### Why Your Fix Works
    When this function is syncronized with a lock only on thread can check the cache at a time and if the key is missing only one thread is spent on that calculation.

### Bug 4: BuggyLogger
### Race Condition Location
Lines 33 in method `Log(string message)`
Lines 49-56 in method `Flush()`
### Shared State Involved
- `_flushedmessages` field accessed by multiple threads
- `_buffer` field accessed by multiple threads
### Why It's a Bug
    The Buffer and the flushed messages are both in shared memory and constantly updated, therefore some threads will most likly read old data. An unsynced flushed function might copy the buffer twice before clearing and an unsynced append might overwrite other appends.
### Your Fix
    My Fix for sycronizing this code was to lock the buffer append method so their was no chance for an append to get overwriten 
    then I locked the flush method so only on thread can RMW the flusedmesseges at a time. This got rid of the chance a thread may copy the buffer before the first thread in the method could clear it.
### Why Your Fix Works
    By ensuring that the flush and append opperations only accept one thread at a time we ensure that multiple appends don't overwrite eachother and multiple flushes don't read old data

### Bug 5: BuggyQueue
### Race Condition Location
Lines 39-49 in method `Enqueue()`
Lines 61-76 in method `Dequeue()`
### Shared State Involved
- `_queue` field accessed by multiple threads
- `_count` field accessed by multiple threads
- `_isCompleted` field accessed by multiple threads
### Why It's a Bug
    Enqueue has multiple threads that can RMW count and the queue at the same time leading to a race condition. Due to the race condition pulse might trigger when no one is waiting. Dequeue has some values outside of the lock that are require for the logic inside the lock which mean that the values that are being RMW aren't reliable. 
### Your Fix
    I locked the RMW on the count and queue in the Enqueue function and extended the lock in the Dequeue functiuon to cover checking the count and subtracting from the queue and count.
### Why Your Fix Works
    My Fix worked because with only one thread updating count or the queue at a time their is no race condtion. 

