
/*
    PocketPotentiostat

    Copyright (C) 2019 Yasuo Matsubara

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation; either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.SolverFoundation.Common;
using Microsoft.SolverFoundation.Solvers;
using Microsoft.SolverFoundation.Services;

namespace Voltammogrammer
{
    class NonlinearRegression
    {
        double[] _decay, _time; double _factor; double _step; double _begin0;
        int _begin, _end;

        public bool perform(ref double[] time, ref double[] decay, double factor, double step, int begin, int end, double begin0, out double capacitance, out double resistance)
        {
            _decay = decay; _time = time;
            _begin = begin; _end = end;
            _factor = factor;
            _step = step;
            _begin0 = begin0;

            Console.WriteLine("NonlinearRegression.perform: time0 = {0}", begin0 / 1000);
            for (int i = _begin; i < _end; i++)
            {
                Console.WriteLine("{0}\t{1}", time[i] / 1000, decay[i] * factor);
            }
            //Console.WriteLine("{0}\t{1}", time[_begin] / 1000, decay[_begin] * factor);
            //Console.WriteLine("{0}\t{1}", time[_end-1] / 1000, decay[_end-1] * factor);

            int vidRow, vidVariable1, vidVariable2, vidVariable3, vidVariable4;
            int vidRow_t, vidVariable1_t, vidVariable2_t, vidVariable3_t, vidVariable4_t;
            double sse, sse_t; INonlinearSolution solution, solution_t;

            solution = _perform_itr(1, 1, out vidRow, out vidVariable1, out vidVariable2, out vidVariable3, out vidVariable4);
            sse = solution.GetValue(vidRow);

            solution_t = _perform_itr(1, 10, out vidRow_t, out vidVariable1_t, out vidVariable2_t, out vidVariable3_t, out vidVariable4_t);
            sse_t = solution_t.GetValue(vidRow_t);
            if (sse_t < sse)
            {
                Console.WriteLine("Global minima found? (1,10): {0}", solution.GetValue(vidRow));
                solution = solution_t; vidRow = vidRow_t; vidVariable1 = vidVariable1_t; vidVariable2 = vidVariable2_t; vidVariable3 = vidVariable3_t; vidVariable4 = vidVariable4_t;
            }

            solution_t = _perform_itr(10, 1, out vidRow_t, out vidVariable1_t, out vidVariable2_t, out vidVariable3_t, out vidVariable4_t);
            sse_t = solution_t.GetValue(vidRow_t);
            if (sse_t < sse)
            {
                Console.WriteLine("Global minima found? (10,1): {0}", solution.GetValue(vidRow));
                solution = solution_t; vidRow = vidRow_t; vidVariable1 = vidVariable1_t; vidVariable2 = vidVariable2_t; vidVariable3 = vidVariable3_t; vidVariable4 = vidVariable4_t;
            }

            solution_t = _perform_itr(10, 10, out vidRow_t, out vidVariable1_t, out vidVariable2_t, out vidVariable3_t, out vidVariable4_t);
            sse_t = solution_t.GetValue(vidRow_t);
            if (sse_t < sse)
            {
                Console.WriteLine("Global minima found? (10,10): {0}", solution.GetValue(vidRow));
                solution = solution_t; vidRow = vidRow_t; vidVariable1 = vidVariable1_t; vidVariable2 = vidVariable2_t; vidVariable3 = vidVariable3_t; vidVariable4 = vidVariable4_t;
            }

            solution_t = _perform_itr(1, 0.1, out vidRow_t, out vidVariable1_t, out vidVariable2_t, out vidVariable3_t, out vidVariable4_t);
            sse_t = solution_t.GetValue(vidRow_t);
            if (sse_t < sse)
            {
                Console.WriteLine("Global minima found? (1,0.1): {0}", solution.GetValue(vidRow));
                solution = solution_t; vidRow = vidRow_t; vidVariable1 = vidVariable1_t; vidVariable2 = vidVariable2_t; vidVariable3 = vidVariable3_t; vidVariable4 = vidVariable4_t;
            }

            solution_t = _perform_itr(0.1, 1, out vidRow_t, out vidVariable1_t, out vidVariable2_t, out vidVariable3_t, out vidVariable4_t);
            sse_t = solution_t.GetValue(vidRow_t);
            if (sse_t < sse)
            {
                solution = solution_t; vidRow = vidRow_t; vidVariable1 = vidVariable1_t; vidVariable2 = vidVariable2_t; vidVariable3 = vidVariable3_t; vidVariable4 = vidVariable4_t;
                Console.WriteLine("Global minima found? (0.1,1): {0}", solution.GetValue(vidRow));
            }

            solution_t = _perform_itr(0.1, 0.1, out vidRow_t, out vidVariable1_t, out vidVariable2_t, out vidVariable3_t, out vidVariable4_t);
            sse_t = solution_t.GetValue(vidRow_t);
            if (sse_t < sse)
            {
                Console.WriteLine("Global minima found? (0.1,0.1): {0}", solution.GetValue(vidRow));
                solution = solution_t; vidRow = vidRow_t; vidVariable1 = vidVariable1_t; vidVariable2 = vidVariable2_t; vidVariable3 = vidVariable3_t; vidVariable4 = vidVariable4_t;
            }



            //solution_t = _perform_itr(1, 10, out vidRow_t, out vidVariable1_t, out vidVariable2_t, out vidVariable3_t);
            //sse_t = solution_t.GetValue(vidRow_t);
            //if (sse_t < sse)
            //{
            //    Console.WriteLine("Global minima found? (1,10): {0}", solution.GetValue(vidRow));
            //    solution = solution_t; vidRow = vidRow_t; vidVariable1 = vidVariable1_t; vidVariable2 = vidVariable2_t; vidVariable3 = vidVariable3_t;
            //}

            //solution_t = _perform_itr(10, 1, out vidRow_t, out vidVariable1_t, out vidVariable2_t, out vidVariable3_t);
            //sse_t = solution_t.GetValue(vidRow_t);
            //if (sse_t < sse)
            //{
            //    Console.WriteLine("Global minima found? (10,1): {0}", solution.GetValue(vidRow));
            //    solution = solution_t; vidRow = vidRow_t; vidVariable1 = vidVariable1_t; vidVariable2 = vidVariable2_t; vidVariable3 = vidVariable3_t;
            //}

            //solution_t = _perform_itr(10, 10, out vidRow_t, out vidVariable1_t, out vidVariable2_t, out vidVariable3_t);
            //sse_t = solution_t.GetValue(vidRow_t);
            //if (sse_t < sse)
            //{
            //    Console.WriteLine("Global minima found? (10,10): {0}", solution.GetValue(vidRow));
            //    solution = solution_t; vidRow = vidRow_t; vidVariable1 = vidVariable1_t; vidVariable2 = vidVariable2_t; vidVariable3 = vidVariable3_t;
            //}

            //solution_t = _perform_itr(1, 0.1, out vidRow_t, out vidVariable1_t, out vidVariable2_t, out vidVariable3_t);
            //sse_t = solution_t.GetValue(vidRow_t);
            //if (sse_t < sse)
            //{
            //    Console.WriteLine("Global minima found? (1,0.1): {0}", solution.GetValue(vidRow));
            //    solution = solution_t; vidRow = vidRow_t; vidVariable1 = vidVariable1_t; vidVariable2 = vidVariable2_t; vidVariable3 = vidVariable3_t;
            //}

            //solution_t = _perform_itr(0.1, 1, out vidRow_t, out vidVariable1_t, out vidVariable2_t, out vidVariable3_t);
            //sse_t = solution_t.GetValue(vidRow_t);
            //if (sse_t < sse)
            //{
            //    solution = solution_t; vidRow = vidRow_t; vidVariable1 = vidVariable1_t; vidVariable2 = vidVariable2_t; vidVariable3 = vidVariable3_t;
            //    Console.WriteLine("Global minima found? (0.1,1): {0}", solution.GetValue(vidRow));
            //}

            //solution_t = _perform_itr(0.1, 0.1, out vidRow_t, out vidVariable1_t, out vidVariable2_t, out vidVariable3_t);
            //sse_t = solution_t.GetValue(vidRow_t);
            //if(sse_t < sse)
            //{
            //    Console.WriteLine("Global minima found? (0.1,0.1): {0}", solution.GetValue(vidRow));
            //    solution = solution_t; vidRow = vidRow_t; vidVariable1 = vidVariable1_t; vidVariable2 = vidVariable2_t; vidVariable3 = vidVariable3_t;
            //}

            //var solver = new NelderMeadSolver();
            //// By CQN solver convention, the vid for a row will always be 0
            //solver.AddRow(null, out vidRow);
            //solver.AddGoal(vidRow, 0, true);
            //// By CQN solver convention, first (and in this case only) variable always has the vid of 1
            //solver.AddVariable(null, out vidVariable1);
            //solver.AddVariable(null, out vidVariable2);
            //solver.AddVariable(null, out vidVariable3);
            ////solver.AddVariable(null, out vidVariable4);

            //Rational rVariable1 = 1; solver.SetValue(vidVariable1, rVariable1);
            //Rational rVariable2 = 1; solver.SetValue(vidVariable2, rVariable2);
            ////Rational rVariable2 = 0.0001; solver.SetValue(vidVariable2, rVariable2);
            //Rational rVariable3 = -1; solver.SetValue(vidVariable3, rVariable3);
            ////Rational rVariable4 = 10.0; solver.SetValue(vidVariable4, rVariable4);

            ////Setting the evaluators
            ////solver.FunctionEvaluator = ((m,r,v,n) => { return Math.Pow(v[1] + _decay[1], 2); });
            //solver.FunctionEvaluator = this.SinxValue;
            ////solver.GradientEvaluator = SinxGradient;

            //var param = new NelderMeadSolverParams();
            ////param.MaximumSearchPoints = 10;

            //INonlinearSolution solution = solver.Solve(param);

            Console.WriteLine("The Result is " + solution.Result + "");
            Console.WriteLine("The minimium value for the error function is " + solution.GetValue(vidRow) + "");
            Console.WriteLine("Ru = " + solution.GetValue(vidVariable1) + "");
            Console.WriteLine("Cd = " + solution.GetValue(vidVariable2) + "");
            Console.WriteLine("C = " + solution.GetValue(vidVariable3) + "");
            Console.WriteLine("deltaTime = " + solution.GetValue(vidVariable4) + "");
            //Console.WriteLine("dt = " + solution.GetValue(vidVariable4) + " ms");

            resistance = solution.GetValue(vidVariable1);
            capacitance = solution.GetValue(vidVariable2);

            switch(solution.Result)
            {
                case NonlinearResult.Feasible:
                case NonlinearResult.LocalOptimal:
                case NonlinearResult.Optimal:
                    return true;
                default:
                    return false;
            }
        }

        private INonlinearSolution _perform_itr(Rational var1_initial, Rational var2_initial, out int vidRow, out int vidVariable1, out int vidVariable2, out int vidVariable3, out int vidVariable4)
        {
            var solver = new NelderMeadSolver();
            // By CQN solver convention, the vid for a row will always be 0
            solver.AddRow(null, out vidRow);
            solver.AddGoal(vidRow, 0, true);
            // By CQN solver convention, first (and in this case only) variable always has the vid of 1
            solver.AddVariable(null, out vidVariable1);
            solver.AddVariable(null, out vidVariable2);
            solver.AddVariable(null, out vidVariable3);
            solver.AddVariable(null, out vidVariable4);
            //solver.AddVariable(null, out vidVariable4);

            Rational rVariable1 = var1_initial; solver.SetValue(vidVariable1, rVariable1);
            Rational rVariable2 = var2_initial; solver.SetValue(vidVariable2, rVariable2);
            //Rational rVariable2 = 0.0001; solver.SetValue(vidVariable2, rVariable2);
            Rational rVariable3 = -0.2; solver.SetValue(vidVariable3, rVariable3);
            Rational rVariable4 = 0.0; solver.SetValue(vidVariable4, rVariable4);

            //Setting the evaluators
            //solver.FunctionEvaluator = ((m,r,v,n) => { return Math.Pow(v[1] + _decay[1], 2); });
            solver.FunctionEvaluator = this.SinxValue;
            //solver.GradientEvaluator = SinxGradient;

            var param = new NelderMeadSolverParams();
            //param.MaximumSearchPoints = 10;

            return solver.Solve(param);
        }

        private double SinxValue(INonlinearModel model, int rowVid, ValuesByIndex values, bool newValues)
        {

            //_decay[0][0] = 1.000;

            // By CQN solver convention, first (and in this case only) variable always has the vid of 1

            double sum = 0.0;

            for(int i = _begin; i < _end; i++)
            {
                sum += Math.Pow
                    (
                        ( (+1 * (_step * 2/1000.0) / values[1]) * Math.Exp(-1.0 * ((_time[i] - _begin0 + 0) / 1000.0) / (values[1] * values[2] / 1000.0)) * 1000.0 + (values[3]) - (_decay[i] * _factor) ),
                        2
                    );
            }

            return (sum);
        }

        private double val(double x1, double x2, double x3)
        {

            //_decay[0][0] = 1.000;

            // By CQN solver convention, first (and in this case only) variable always has the vid of 1

            double sum = 0.0; double t = 0.0;

            for (int i = _begin; i < _end; i++)
            {
                t = Math.Pow(((-0.05 / x1) * Math.Exp(-1.0 * ((_time[i] - _time[_begin] + 10.0) / 1000.0) / (x1 * x2)) * 1000000.0 + x3 - (_decay[i] * _factor)), 2);
                sum += t;
            }

            return (sum);
        }

        //private static void SinxGradient(INonlinearModel model, int rowVid, ValuesByIndex values, bool newValues, ValuesByIndex gradient)
        //{
        //    // By CQN solver convention, first (and only) variable always has the vid of 1
        //    gradient[1] = Math.Cos(values[1]);
        //}
    }
}
