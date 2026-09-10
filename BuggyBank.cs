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
    
    public BuggyBank(decimal initialBalance)
    {
        _balance = initialBalance;
    }

    public decimal Balance => _balance;

    public void Deposit(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive");

        // BUG: This is not atomic!
        decimal current = _balance;
        // Simulate some processing time
        Thread.SpinWait(100);
        _balance = current + amount;
    }

    public bool Withdraw(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive");

        // BUG: Check-then-act race condition
        if (_balance >= amount)
        {
            decimal current = _balance;
            
            // Simulate some processing time
            Thread.SpinWait(100);
            
            _balance = current - amount;
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
