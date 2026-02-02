# 000 - Prompt approach request

**Date:** 2026-02-02  
**Type:** chatgpt request with deep thinking on how to approach the problem and build an initial prompt with specifications and an approach that ensures fairness in the process

---

## Original Request


ok, now I want the prompt for the Coder Agent to: Create a .NET 10 with C# project that imports excel with the entries of the survey, loads the name, email, the laptop question and 10min commitmetnt, attendance request for the three workshops, and after wards the ranking of workshops in importance order, Workshop 2 – AI Architecture Critic;Workshop 1 – Secure Coding Literacy for Vibe Coders;Workshop 3 – Build a Pizza Ordering Agent; I wan this in an internal list for example. Then I want a process afterwards to select fairnesly the attendees for each workshop, taking into account if they have required attendance to it or not. I would like to give prioritiy to the ones that have put the ranking higher (maybe assigning more chances to that workshop) and also do the assignment in waves so if somebody is already attending a workshop, give priority to the people that is not attending any workshop yet and so on. How would you do this process? I'd suggest generating the attendees for the 1st one and putting the rest as waitinglist and then the second workshop having into account who is already attending a workshop. and so on. I am attaching an example of the excel with the corresponding fields. Please think carefully how would you do this process for the random selection of the attendees with some conditions and "chances" so it is a fair process and we can distribute properly the workshop spots. Finally I would like to output the tables of the workshops in an excel with 3 tabs, one per workshop. stating who is an attendee and who is in the waiting list and in what order (assign it randomly)