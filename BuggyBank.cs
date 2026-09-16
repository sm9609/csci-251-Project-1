// Samuel Mensah
// CSCI 251 - Project 1: Race Condition Detective
// Bug 1: BuggyBank - Fix the race condition in this file

namespace RaceConditionDetective;

/// <summary>
/// A simple bank account that supports deposits and withdrawals.
/// BUG: There is a race condition that causes incorrect balances
/// when multiple threads transfer money simultaneously.
/// </summary>
public class BuggyBank
{
    private decimal _balance;
    private bool isWriting = false;
    private bool isReading = false;
    private readonly object _sync = new object();
    
    public BuggyBank(decimal initialBalance)
    {
        _balance = initialBalance;
    }

    public decimal Balance => _balance;
    private decimal ReadBalance()
    {   
        decimal current;
        lock(_sync){
            while(isWriting)
            Monitor.Wait(_sync);
                
            current = this.Balance;
        }
        return current;
    }
    public void Deposit(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive");

        // BUG: This is not atomic!
        

        // Simulate some processing time
        Thread.SpinWait(100);
        lock (_sync)
        {
            while (isReading || isWriting)
            {
                Monitor.Wait(_sync);
            }
            _balance =+ amount;
            Monitor.PulseAll(_sync);
        }
        
    }

    public bool Withdraw(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive");

        // BUG: Check-then-act race condition
        lock(_sync){}
        if (_balance >= amount)
        {
            // Simulate some processing time
            Thread.SpinWait(100);
            
            _balance =- amount;
            return true;
        }
        return false;
    }

    public void Transfer(BuggyBank destination, decimal amount)
    {
        if (Withdraw(amount))
        {
            destination.Deposit(amount);
        }
    }

    
}
