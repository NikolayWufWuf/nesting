﻿using NestingLibPort.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


//package com.qunhe.util.nest.util;

//import com.qunhe.util.nest.data.*;
//import java.util.List;
namespace NestingLibPort.Util
{
    public class NfpUtil
    {

        /**
         * 获取一对多边形，并生成nfp
         * @param pair
         * @param config
         * @return
         */
        public static ParallelData nfpGenerator(NfpPair pair, Config config)
        {
            bool searchEdges = config.isCONCAVE();
            bool useHoles = config.isUSE_HOLE();

            NestPath A = GeometryUtil.rotatePolygon2Polygon(pair.getA(), pair.getKey().getArotation());
            NestPath B = GeometryUtil.rotatePolygon2Polygon(pair.getB(), pair.getKey().getBrotation());

            List<NestPath> nfp;
            if (pair.getKey().isInside())
            {
                if (GeometryUtil.isRectangle(A, 0.001))
                {
                    nfp = GeometryUtil.noFitPolygonRectangle(A, B);
                    if (nfp == null)
                    {

                    }
                }
                else
                {
                    nfp = GeometryUtil.noFitPolygon(A, B, true, searchEdges);
                }
                if (nfp != null && nfp.Count > 0)
                {
                    for (int i = 0; i < nfp.Count; i++)
                    {
                        if (GeometryUtil.polygonArea(nfp[i]) > 0)
                        {
                            nfp[i].reverse();
                        }
                    }
                }
                else
                {
                    //Warning on null inner NFP
                }
            }
            else
            {
                int count = 0;
                if (searchEdges)
                {

                    // NFP Generator TODO  double scale contorl
                    nfp = GeometryUtil.noFitPolygon(A, B, false, searchEdges);
                    if (nfp == null)
                    {

                    }
                }
                else
                {

                    nfp = GeometryUtil.minkowskiDifference(A, B);
                }
                // sanity check
                if (nfp == null || nfp.Count == 0)
                {

                    return null;
                }
                for (int i = 0; i < nfp.Count; i++)
                {
                    if (!searchEdges || i == 0)
                    {
                        if (Math.Abs(GeometryUtil.polygonArea(nfp[i])) < Math.Abs(GeometryUtil.polygonArea(A)))
                        {
                            nfp.RemoveAt(i);

                            return null;
                        }
                    }
                }
                if (nfp.Count == 0)
                {

                    return null;
                }

                for (int i = 0; i < nfp.Count; i++)
                {
                    if (GeometryUtil.polygonArea(nfp[i]) > 0)
                    {
                        nfp[i].reverse();
                    }

                    if (i > 0)
                    {
                        if ((bool)GeometryUtil.pointInPolygon(nfp[i].get(0), nfp[0]))
                        {
                            if (GeometryUtil.polygonArea(nfp[i]) < 0)
                            {
                                nfp[i].reverse();
                            }
                        }
                    }
                }

                if (useHoles && A.getChildren().Count > 0)
                {
                    Bound Bbounds = GeometryUtil.getPolygonBounds(B);
                    for (int i = 0; i < A.getChildren().Count; i++)
                    {
                        Bound Abounds = GeometryUtil.getPolygonBounds(A.getChildren()[i]);

                        if (Abounds.width > Bbounds.width && Abounds.height > Bbounds.height)
                        {

                            List<NestPath> cnfp = GeometryUtil.noFitPolygon(A.getChildren()[i], B, true, searchEdges);
                            // ensure all interior NFPs have the same winding direction

                            if (cnfp != null && cnfp.Count > 0)
                            {

                                for (int j = 0; j < cnfp.Count; j++)
                                {
                                    if (GeometryUtil.polygonArea(cnfp[j]) < 0)
                                    {
                                        cnfp[j].reverse();
                                    }
                                    nfp.Add(cnfp[j]);
                                }
                            }

                        }
                    }
                }
            }
            if (nfp == null)
            {

            }
            return new ParallelData(pair.getKey(), nfp);
        }
    }

}
