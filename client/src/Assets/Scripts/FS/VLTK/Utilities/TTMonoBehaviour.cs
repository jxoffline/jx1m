using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Extending MonoBehaviour to add some extra functionality
/// Exception handling from: http://twistedoakstudios.com/blog/Post83_coroutines-more-than-you-want-to-know
/// 
/// 2013 Tim Tregubov
/// </summary>
public class TTMonoBehaviour : MonoBehaviour
{
    private LockQueue LockedCoroutineQueue { get; set; }

    public delegate void CoroutineExceptionHandler();

    /// <summary>
    /// Coroutine with return value AND exception handling on the return value. 
    /// </summary>
    public Coroutine<T> StartCoroutine<T>(IEnumerator coroutine)
    {
        Coroutine<T> coroutineObj = new Coroutine<T>();
        coroutineObj.coroutine = base.StartCoroutine(coroutineObj.InternalRoutine(coroutine));
        return coroutineObj;
    }

    /// <summary>
    /// Coroutine with return value AND exception handling on the return value. 
    /// </summary>
    public Coroutine<T> StartCoroutine<T>(IEnumerator coroutine, CoroutineExceptionHandler exceptionHandler)
    {
        Coroutine<T> coroutineObj = new Coroutine<T>();
        coroutineObj.coroutine = base.StartCoroutine(coroutineObj.InternalRoutine(coroutine));
        coroutineObj.coroutineException = exceptionHandler;
        return coroutineObj;
    }

    /// <summary>
    /// Lockable coroutine. Can either wait for a previous coroutine to finish or a timeout or just bail if previous one isn't done.
    /// Caution: the default timeout is 10 seconds. Coroutines that timeout just drop so if its essential increase this timeout.
    /// Set waitTime to 0 for no wait
    /// </summary>
    public Coroutine<T> StartCoroutine<T>(IEnumerator coroutine, string lockID, float waitTime = 10f)
    {
        if (LockedCoroutineQueue == null) LockedCoroutineQueue = new LockQueue();
        Coroutine<T> coroutineObj = new Coroutine<T>(lockID, waitTime, LockedCoroutineQueue);
        coroutineObj.coroutine = base.StartCoroutine(coroutineObj.InternalRoutine(coroutine));
        return coroutineObj;
    }

    /// <summary>
    /// Coroutine with return value AND exception handling AND lockable
    /// </summary>
    public class Coroutine<T>
    {
        private T returnVal;
        private Exception e;
        private string lockID;
        private float waitTime;

        private LockQueue lockedCoroutines; //reference to objects lockdict
        private bool lockable;

        public Coroutine coroutine;
        public T Value
        {
            get
            {
                if (e != null)
                {
                    throw e;
                }
                return returnVal;
            }
        }

        public CoroutineExceptionHandler coroutineException = null;

        public Coroutine() { lockable = false; }
        public Coroutine(string lockID, float waitTime, LockQueue lockedCoroutines)
        {
            this.lockable = true;
            this.lockID = lockID;
            this.lockedCoroutines = lockedCoroutines;
            this.waitTime = waitTime;
        }

        public IEnumerator InternalRoutine(IEnumerator coroutine)
        {
            if (lockable && lockedCoroutines != null)
            {
                if (lockedCoroutines.Contains(lockID))
                {
                    if (waitTime == 0f)
                    {
                        //Debug.Log(this.GetType().Name + ": coroutine already running and wait not requested so exiting: " + lockID);
                        yield break;
                    }
                    else
                    {
                        //Debug.Log(this.GetType().Name + ": previous coroutine already running waiting max " + waitTime + " for my turn: " + lockID);
                        float starttime = Time.time;
                        float counter = 0f;
                        lockedCoroutines.Add(lockID, coroutine);
                        while (!lockedCoroutines.First(lockID, coroutine) && (Time.time - starttime) < waitTime)
                        {
                            yield return null;
                            counter += Time.deltaTime;
                        }
                        if (counter >= waitTime)
                        {
                            string error = this.GetType().Name + ": coroutine " + lockID + " bailing! due to timeout: " + counter;
                            KTDebug.LogError(error);
                            this.e = new Exception(error);
                            lockedCoroutines.Remove(lockID, coroutine);
                            if (null != coroutineException)
                            {
                                coroutineException();
                            }
                            yield break;
                        }
                    }
                }
                else
                {
                    lockedCoroutines.Add(lockID, coroutine);
                }
            }

            while (true)
            {
                try
                {
                    if (!coroutine.MoveNext())
                    {
                        if (lockable) lockedCoroutines.Remove(lockID, coroutine);
                        yield break;
                    }
                }
                catch (Exception e)
                {
                    this.e = e;
                    //Debug.LogError(this.GetType().Name + ": caught Coroutine exception! " + e.Message + "\n" + e.StackTrace); 
                    KTDebug.LogException(e);
                    if (lockable) lockedCoroutines.Remove(lockID, coroutine);
                    if (null != coroutineException)
                    {
                        coroutineException();
                    }
                    yield break;
                }

                object yielded = coroutine.Current;
                if (yielded != null && yielded.GetType() == typeof(T))
                {
                    returnVal = (T)yielded;
                    if (lockable) lockedCoroutines.Remove(lockID, coroutine);
                    yield break;
                }
                else
                {
                    yield return coroutine.Current;
                }
            }
        }
    }


    /// <summary>
    /// coroutine lock and queue
    /// </summary>
    public class LockQueue
    {
        private Dictionary<string, List<IEnumerator>> LockedCoroutines { get; set; }

        public LockQueue()
        {
            LockedCoroutines = new Dictionary<string, List<IEnumerator>>();
        }

        /// <summary>
        /// check if LockID is locked
        /// </summary>
        public bool Contains(string lockID)
        {
            return LockedCoroutines.ContainsKey(lockID);
        }

        /// <summary>
        /// check if given coroutine is first in the queue
        /// </summary>
        public bool First(string lockID, IEnumerator coroutine)
        {
            bool ret = false;
            if (Contains(lockID))
            {
                if (LockedCoroutines[lockID].Count > 0)
                {
                    ret = LockedCoroutines[lockID][0] == coroutine;
                }
            }
            return ret;
        }

        /// <summary>
        /// Add the specified lockID and coroutine to the coroutine lockqueue
        /// </summary>
        public void Add(string lockID, IEnumerator coroutine)
        {
            if (!LockedCoroutines.ContainsKey(lockID))
            {
                LockedCoroutines.Add(lockID, new List<IEnumerator>());
            }

            if (!LockedCoroutines[lockID].Contains(coroutine))
            {
                LockedCoroutines[lockID].Add(coroutine);
            }
        }

        /// <summary>
        /// Remove the specified coroutine and queue if empty
        /// </summary>
        public bool Remove(string lockID, IEnumerator coroutine)
        {
            bool ret = false;
            if (LockedCoroutines.ContainsKey(lockID))
            {
                if (LockedCoroutines[lockID].Contains(coroutine))
                {
                    ret = LockedCoroutines[lockID].Remove(coroutine);
                }

                if (LockedCoroutines[lockID].Count == 0)
                {
                    ret = LockedCoroutines.Remove(lockID);
                }
            }
            return ret;
        }

    }

}

