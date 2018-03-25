using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WcfDataService
{
    public  class DisableDayObject : IEnumerable, IEnumerator
    {
     public int NP1=0;
     public int NP2=0;
     public int NP3=0;
     public int NP4=0;
       static int[] ints=new int[4];
        int index = -1;
         public  DisableDayObject(int NP1,int NP2,int NP3,int NP4)
        {
            this.NP1 = NP1;
            this.NP2 = NP2;
            this.NP3 = NP3;
            this.NP4 = NP4;
            ints[0] = NP1;
            ints[1] = NP2;
            ints[2] = NP3;
            ints[3] = NP4;

        }
  
        public IEnumerator GetEnumerator()
        {
            return this;
        }


        public bool MoveNext()
        {
            if (index == ints.Length - 1)
            {
                Reset();
                return false;
            }

            index++;
            return true;
        }


        public int Sum()
        {
            return NP1 + NP2 + NP3 + NP4;
        }

        public void Reset()
        {
            index = -1;
        }

        public object Current
        {
            get
            {
                return ints[index];
            }
        }


        public enum DDO 
        {
            NP1 = 61,
            NP2 = 62,
            NP3 = 63,
            NP4 = 64 
        }

    }
}